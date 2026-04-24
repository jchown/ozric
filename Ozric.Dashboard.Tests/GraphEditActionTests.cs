using Ozric.Dashboard.Components;
using Xunit;

namespace Ozric.Dashboard.Tests;

public class GraphEditActionTests
{
    private sealed class OrderSpy : GraphEditAction
    {
        private readonly string _id;
        private readonly List<string> _log;

        public OrderSpy(string id, List<string> log)
        {
            _id = id;
            _log = log;
        }

        public void Do(AreaGraphView editor) => _log.Add($"do:{_id}");
        public void Undo(AreaGraphView editor) => _log.Add($"undo:{_id}");
    }

    [Fact]
    public void EditActions_Do_RunsForward_Undo_RunsReverse()
    {
        var log = new List<string>();
        var composite = GraphEditAction.Build(new GraphEditAction[]
        {
            new OrderSpy("a", log),
            new OrderSpy("b", log),
            new OrderSpy("c", log),
        });

        composite.Do(null!);
        composite.Undo(null!);

        Assert.Equal(
            new[] { "do:a", "do:b", "do:c", "undo:c", "undo:b", "undo:a" },
            log);
    }
}
