using Ozric.Engine.Graph;
using Ozric.Engine;
using Ozric.Engine.Live;

namespace Ozric.Service;

/// <summary>
/// Interface to the engine
/// </summary>
public interface IOzricService
{
    public Task AwaitStarted();

    EngineStatus Status { get; }
    
    Graph Graph { get; }
    
    IHome Home { get; }
    
    ICommandSender CommandSender { get; }

    Task Start(CancellationToken token);
    
    void SetPaused(bool statusPaused);

    Task SaveGraph(Graph graph);

    /// <summary>
    /// Persist an edited graph and make the running engine use it. After this returns,
    /// <see cref="Graph"/> reflects the edits (and so does a reloaded page, a download,
    /// or a restart of the add-on). The caller keeps ownership of <paramref name="graph"/>
    /// and may carry on editing it; the engine runs its own copy.
    /// </summary>
    Task ApplyGraph(Graph graph);

    Task Restart(Graph graph);
    
    void Subscribe(Pin.Changed pinChanged, Alert.Changed alertChanged);
    
    void Unsubscribe(Pin.Changed pinChanged, Alert.Changed alertChanged);
}