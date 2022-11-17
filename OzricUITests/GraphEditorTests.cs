using System;
using System.Threading.Tasks;
using Blazor.Diagrams.Core.Geometry;
using Bunit;
using OzricEngine;
using OzricEngine.Nodes;
using OzricUI;
using OzricUI.Components;
using OzricUI.Shared;
using Xunit;

namespace OzricUITests;

public class GraphEditorTests
{
    /*
    [Fact]
    public void CanInitialiseEmptyEditor()
    {
        using var ctx = new TestContext();

        CreateEditor(ctx);
    }
    */

    [Fact]
    public async Task CanAddWithHistory()
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
}