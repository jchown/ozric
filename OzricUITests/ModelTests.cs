using NUnit.Framework;
using OzricEngine;
using OzricEngine.logic;
using OzricUI;

namespace OzricUITests;

public class ModelTests
{
    [Test]
    public void TestCloneGraph()
    {
        var graph1 = new Graph();
        graph1.AddNode(new Light("light-1", "light-1"));
        graph1.AddNode(new Constant("constant-1", ColorRGB.GREEN));
        graph1.Connect("constant-1", "value", "light-1", "color");

        var graph2 = Json.Clone(graph1);
        Assert.AreEqual(2, graph2.nodes.Count);
        Assert.AreEqual(ColorRGB.GREEN, graph2.nodes["constant-1"].GetOutputValue("value"));

        Assert.AreEqual(graph1, graph2);
    }

    [Test]
    public void TestCloneLayout()
    {
        var layout1 = new GraphLayout();
        layout1.nodeLayout["node-1"] = new LayoutPoint(3, 4);

        var layout2 = Json.Clone(layout1);
        Assert.AreEqual(1, layout2.nodeLayout.Count);
        Assert.AreEqual(3, layout2.nodeLayout["node-1"].x);
        Assert.AreEqual(4, layout2.nodeLayout["node-1"].y);

        Assert.AreEqual(layout1, layout2);
    }

}