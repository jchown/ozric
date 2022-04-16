using NUnit.Framework;
using OzricEngine;
using OzricUI;

namespace OzricUITests;

public class ModelTests
{
    [Test]
    public void TestClone()
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