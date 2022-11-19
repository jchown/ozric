using System;
using System.Threading.Tasks;
using Blazor.Diagrams.Core.Geometry;
using Bunit;
using OzricEngine;
using OzricEngine.Nodes;
using OzricEngine.Values;
using OzricUI;
using OzricUI.Components;
using OzricUI.Shared;
using Xunit;
using Xunit.Abstractions;

namespace OzricUITests;

public class GraphEditorTests
{
    public GraphEditorTests(ITestOutputHelper testOutputHelper)
    {
        OzricObject.LogOutput = (line) => testOutputHelper.WriteLine(line);
    }
    
    /*
    [Fact]
    public void CanInitialiseEmptyEditor()
    {
        using var ctx = new TestContext();

        CreateEditor(ctx);
    }
    */

    [Fact]
    public async Task CanAddNodeWithHistory()
    {
        using var ctx = new TestContext();

        var editorComponent = CreateEditor(ctx);
        var editor = editorComponent.Instance;
        var node = new SkyBrightness();

        await TestHistory(editorComponent, new GraphEditAction.AddNode(node), () =>
        {
            Assert.True(editor.Graph.nodes.Count == 0);
            Assert.True(editor.diagram.Nodes.Count == 0);
        }, () =>
        {
            Assert.True(editor.Graph.nodes.Count == 1);
            Assert.True(editor.diagram.Nodes.Count == 1);
        });
    }
    
    [Fact]
    public async Task CanRemoveWithHistory()
    {
        using var ctx = new TestContext();

        var editorComponent = CreateEditor(ctx);
        var editor = editorComponent.Instance;
        
        var node = new SkyBrightness();
        await DoEdit(editorComponent, new GraphEditAction.AddNode(node));
  
        await TestHistory(editorComponent, new GraphEditAction.RemoveNode(node), () =>
        {
            Assert.True(editor.Graph.nodes.Count == 1);
            Assert.True(editor.diagram.Nodes.Count == 1);
        }, () =>
        {
            Assert.True(editor.Graph.nodes.Count == 0);
            Assert.True(editor.diagram.Nodes.Count == 0);
        });
    }

    [Fact]
    public async Task CanLinkNodesWithHistory()
    {
        using var ctx = new TestContext();

        var editorComponent = CreateEditor(ctx);
        var editor = editorComponent.Instance;
        var number = new Constant("one", new Number(1.0f));
        var compare = NumberCompare.Of("greater-than-half", NumberCompare.Comparator.GreaterThan, 0.5f);
        var edge = new Edge(new OutputSelector(number.id, Constant.OutputName), new InputSelector(compare.id, NumberCompare.InputName));

        await DoEdit(editorComponent, new GraphEditAction.AddNode(number));
        await DoEdit(editorComponent, new GraphEditAction.AddNode(compare));
            
        await TestHistory(editorComponent, new GraphEditAction.AddEdge(edge), () =>
        {
            Assert.True(editor.Graph.edges.Count == 0);
            Assert.True(editor.diagram.Links.Count == 0);
            Assert.True(editor.diagram.Nodes[0].Ports[0].Links.Count == 0);
            Assert.True(editor.diagram.Nodes[1].Ports[0].Links.Count == 0);
        }, () =>
        {
            Assert.True(editor.Graph.edges.Count == 1);
            Assert.True(editor.diagram.Links.Count == 1);
            Assert.True(editor.diagram.Nodes[0].Ports[0].Links.Count == 1);
            Assert.True(editor.diagram.Nodes[1].Ports[0].Links.Count == 1);
        });
    }

    [Fact]
    public async Task CanRemoveLinkedNodeWithHistory()
    {
        using var ctx = new TestContext();

        var editorComponent = CreateEditor(ctx);
        var editor = editorComponent.Instance;
        
        var number = new Constant("one", new Number(1.0f));
        var compare = NumberCompare.Of("greater-than-half", NumberCompare.Comparator.GreaterThan, 0.5f);
        var edge = new Edge(new OutputSelector(number.id, Constant.OutputName), new InputSelector(compare.id, NumberCompare.InputName));

        await DoEdit(editorComponent, new GraphEditAction.AddNode(number));
        await DoEdit(editorComponent, new GraphEditAction.AddNode(compare));
        await DoEdit(editorComponent, new GraphEditAction.AddEdge(edge));

        await editorComponent.InvokeAsync(() => editor.Select(compare));
            
        await TestHistory(editorComponent, GraphEditState.Command.Delete, () =>
        {
            Assert.True(editor.Graph.edges.Count == 1);
            Assert.True(editor.diagram.Links.Count == 1);
            Assert.True(editor.Graph.nodes.Count == 2);
            Assert.True(editor.diagram.Nodes.Count == 2);
        }, () =>
        {
            Assert.True(editor.Graph.edges.Count == 0);
            Assert.True(editor.diagram.Links.Count == 0);
            Assert.True(editor.Graph.nodes.Count == 1);
            Assert.True(editor.diagram.Nodes.Count == 1);
        });
    }

    private async Task DoEdit(IRenderedComponent<GraphEditor> component, GraphEditAction action)
    {
        await component.InvokeAsync(() => component.Instance.GraphEditState.DoAction(action));
    }

    private async Task TestHistory(IRenderedComponent<GraphEditor> editorComponent, GraphEditAction action, Action assertPreState, Action assertPostState)
    {
        var editor = editorComponent.Instance;

        assertPreState();
        await editorComponent.InvokeAsync(() => editor.GraphEditState.DoAction(action));
        assertPostState();
        await editorComponent.InvokeAsync(() => editor.GraphEditState.DoCommand(GraphEditState.Command.Undo));
        assertPreState();
        await editorComponent.InvokeAsync(() => editor.GraphEditState.DoCommand(GraphEditState.Command.Redo));
        assertPostState();
        await editorComponent.InvokeAsync(() => editor.GraphEditState.DoCommand(GraphEditState.Command.Undo));
        assertPreState();
    }

    private async Task TestHistory(IRenderedComponent<GraphEditor> editorComponent, GraphEditState.Command command, Action assertPreState, Action assertPostState)
    {
        var editor = editorComponent.Instance;

        assertPreState();
        await editorComponent.InvokeAsync(() => editor.GraphEditState.DoCommand(command));
        assertPostState();
        await editorComponent.InvokeAsync(() => editor.GraphEditState.DoCommand(GraphEditState.Command.Undo));
        assertPreState();
        await editorComponent.InvokeAsync(() => editor.GraphEditState.DoCommand(GraphEditState.Command.Redo));
        assertPostState();
        await editorComponent.InvokeAsync(() => editor.GraphEditState.DoCommand(GraphEditState.Command.Undo));
        assertPreState();
    }

    private static IRenderedComponent<GraphEditor> CreateEditor(TestContext ctx)
    {
        ctx.JSInterop.Setup<Rectangle>("ZBlazorDiagrams.getBoundingClientRect", _ => true).SetResult(new Rectangle(10, 10, 810, 610));
        ctx.JSInterop.SetupVoid("ZBlazorDiagrams.observe", _ => true);

        var graph = new Graph();
        var layout = new GraphLayout();
        var state = new GraphEditState();

        var parameters = new[]
        {
            ComponentParameter.CreateParameter(nameof(GraphEditor.Graph), graph),
            ComponentParameter.CreateParameter(nameof(GraphEditor.GraphLayout), layout),
            ComponentParameter.CreateParameter(nameof(GraphEditor.GraphEditState), state)
        };

        return ctx.RenderComponent<GraphEditor>(parameters);
    }
    
    public static async Task<CommandBatcher> ProcessNodes(Graph graph, Home home, Func<Node, Context, Task> nodeProcessor)
    {
        var context = new MockContext(home);

        foreach (var nodeID in graph.GetNodesInOrder())
        {
            var node = graph.GetNode(nodeID)!;

            if (node.IsReady())
            {
                await nodeProcessor(node, context);

                graph.CopyNodeOutputValues(node, context);
            }
        }

        return context.commands;
    }

}