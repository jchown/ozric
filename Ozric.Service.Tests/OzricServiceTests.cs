using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ozric.Engine;
using Ozric.Engine.Graph;
using Ozric.Engine.Graph.Logic;
using Ozric.Engine.Live;
using Ozric.Engine.Messages;
using Ozric.Engine.Model;
using Ozric.Engine.Nodes;
using Ozric.Engine.Values;
using Xunit;
using Graph = Ozric.Engine.Graph.Graph;
using ValueType = Ozric.Engine.Values.ValueType;

namespace Ozric.Service.Tests;

/// <summary>
/// What happens to the graph between the editor, the running engine and the disk.
/// The bug these guard against: an edit was saved to disk but the running engine kept the old graph,
/// so reloading the page (which shows the running graph) brought deleted links back.
/// </summary>
public class OzricServiceTests : IDisposable
{
    private const string Area = "test-area";
    private const string SwitchEdge = "sw/value:choice/switch";
    private const string OnEdge = "on/value:choice/on";
    private const string OffEdge = "off/value:choice/off";

    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"ozric-service-tests-{Guid.NewGuid():N}");
    private readonly List<OzricService> _services = new();

    private string GraphFile => Path.Combine(_dir, "graph.json");

    public OzricServiceTests()
    {
        Directory.CreateDirectory(_dir);
    }

    public void Dispose()
    {
        foreach (var service in _services)
            service.Stop(CancellationToken.None).GetAwaiter().GetResult();

        Directory.Delete(_dir, true);
    }

    [Fact]
    public async Task ApplyGraph_MakesTheEditedGraphTheRunningGraph_AndSavesIt()
    {
        var service = StartService(BuildGraph());

        var edited = Json.Clone(service.Graph);
        edited.Disconnect("sw", "value", "choice", "switch");

        await service.ApplyGraph(edited);

        //  This is what a reloaded page, a download and the Restart button see

        Assert.DoesNotContain(SwitchEdge, service.Graph.edges.Keys);
        Assert.Equal(new[] { OffEdge, OnEdge }, service.Graph.edges.Keys.Order());

        //  This is what the next start of the add-on sees

        var saved = Graph.Deserialize(await File.ReadAllTextAsync(GraphFile));
        Assert.Equal(new[] { OffEdge, OnEdge }, saved.edges.Keys.Order());
    }

    [Fact]
    public async Task ApplyGraph_GivesTheEngineItsOwnCopy()
    {
        var service = StartService(BuildGraph());

        var edited = Json.Clone(service.Graph);
        edited.Disconnect("sw", "value", "choice", "switch");

        await service.ApplyGraph(edited);

        //  The editor carries on changing "edited" until it is saved again; that mustn't reach the engine

        Assert.NotSame(edited, service.Graph);
        edited.Disconnect("on", "value", "choice", "on");
        Assert.Contains(OnEdge, service.Graph.edges.Keys);
    }

    [Fact]
    public async Task ApplyGraph_KeepsTheEngineRunning()
    {
        var service = StartService(BuildGraph());

        var edited = Json.Clone(service.Graph);
        edited.AddNode(new GraphConstant("extra", new Number(3)) { area_id = Area });

        await service.ApplyGraph(edited);

        //  Nodes only get a value once the new engine's main loop has initialised them

        await WaitFor(() => service.Graph.GetNode("extra")?.GetOutput("value").value != null);
        await WaitFor(() => service.Graph.GetNode("choice")!.GetOutput("output").value != null);
    }

    [Fact]
    public async Task ApplyGraph_KeepsSubscribers()
    {
        var service = StartService(BuildGraph());

        var changed = new List<string>();
        service.Subscribe((nodeID, _, _) => { lock (changed) changed.Add(nodeID); }, _ => { });

        var edited = Json.Clone(service.Graph);
        edited.AddNode(new GraphBinaryChoice("extra", ValueType.Number) { area_id = Area });
        edited.Connect("on", "value", "extra", "on");

        await service.ApplyGraph(edited);

        //  Only the new engine knows about "extra", and its input changes from nothing to a value

        await WaitFor(() => { lock (changed) return changed.Contains("extra"); });
    }

    [Fact]
    public async Task ApplyGraph_KeepsTheEnginePaused_IfItWasPaused()
    {
        var service = StartService(BuildGraph());
        service.SetPaused(true);

        await service.ApplyGraph(Json.Clone(service.Graph));

        Assert.True(service.Status.paused);
    }

    [Fact]
    public async Task ApplyGraph_DoesNotHang_ForANodeWithNoAreaAndNoConnections()
    {
        var service = StartService(BuildGraph());

        //  Like a logic node that's just been added to the graph and not wired up yet

        var edited = Json.Clone(service.Graph);
        edited.AddNode(new GraphBinaryChoice("lonely", ValueType.Number));

        var apply = service.ApplyGraph(edited);
        var winner = await Task.WhenAny(apply, Task.Delay(TimeSpan.FromSeconds(10)));

        Assert.Same(apply, winner);
        await apply;
        Assert.Equal(IHome.GlobalAreaId, service.Graph.GetNode("lonely")!.area_id);
    }

    [Fact]
    public async Task ApplyGraph_BeforeTheServiceIsRunning_ThrowsAndWritesNothing()
    {
        var service = new OzricService(GraphFile);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ApplyGraph(BuildGraph()));

        Assert.False(File.Exists(GraphFile));
    }

    [Fact]
    public async Task TheEngineCanBeStoppedAndStartedAgain()
    {
        //  This is what Restart does (but with a new connection to Home Assistant in the middle)

        var service = StartService(BuildGraph());
        await WaitFor(() => service.Graph.GetNode("choice")!.GetOutput("output").value != null);

        await service.Stop(CancellationToken.None);
        service.Run(new StubHome(), new StubComms(), BuildGraph());

        await WaitFor(() => service.Graph.GetNode("choice")!.GetOutput("output").value != null);
    }

    private OzricService StartService(Graph graph)
    {
        var service = new OzricService(GraphFile);
        _services.Add(service);
        service.Run(new StubHome(), new StubComms(), graph);
        return service;
    }

    /// <summary>
    /// A switch choosing between two numbers
    /// </summary>
    private static Graph BuildGraph()
    {
        var graph = new Graph();
        graph.AddNode(new GraphConstant("sw", new Binary(true)) { area_id = Area });
        graph.AddNode(new GraphConstant("on", new Number(1)) { area_id = Area });
        graph.AddNode(new GraphConstant("off", new Number(0)) { area_id = Area });
        graph.AddNode(new GraphBinaryChoice("choice", ValueType.Number) { area_id = Area });
        graph.Connect("sw", "value", "choice", "switch");
        graph.Connect("on", "value", "choice", "on");
        graph.Connect("off", "value", "choice", "off");
        return graph;
    }

    private static async Task WaitFor(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (!condition())
        {
            Assert.True(DateTime.UtcNow < deadline, "Timed out waiting for the engine");
            await Task.Delay(20);
        }
    }

    private class StubComms : IComms
    {
        public CommsStatus Status => new();

        public Task Connect() => Task.CompletedTask;

        public Task<TResponse> SendCommand<TResponse>(ClientCommand command, int millisecondsTimeout = IComms.DefaultCommandTimeoutMilliseconds, bool verbose = false) where TResponse : ServerResponse, new()
            => throw new NotSupportedException();

        public List<ServerEvent> TakePendingEvents(int millisToWait)
        {
            Thread.Sleep(millisToWait);
            return new List<ServerEvent>();
        }

        public void OnSentMessage(IComms.MessageHandler action) { }
        public void OnSentJson(IComms.JsonHandler action) { }
        public void OnReceivedJson(IComms.JsonHandler action) { }
        public void Dispose() { }
    }

    private class StubHome : IHome
    {
        public HeartbeatStatus SyncStatus => new();
        public DateTime GetTime() => DateTime.Now;
        public void SetUpdatedTime(string entityId) { }
        public bool OnEventStateChanged(EventStateChanged stateChanged) => false;
        public List<EntityState> GetEntityStates() => new();
        public List<AreaConfig> GetAreas() => new();
        public List<EntityState> GetEntityStates(List<string> entityIDs) => new();
        public EntityState? GetEntityState(string entityId) => null;
        public bool CanUpdateEntity(EntityState entityState) => true;
        public List<DeviceConfig> GetDevicesInArea(string areaId) => new();
        public List<EntityConfig> GetEntitiesInArea(string areaId) => new();
        public ReadOnlyCollection<EntityConfig> GetEntityConfigs() => new(new List<EntityConfig>());
        public ReadOnlyCollection<DeviceConfig> GetDeviceConfigs() => new(new List<DeviceConfig>());
    }
}
