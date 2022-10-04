using System;
using OzricEngineTests;
using Xunit;
using OzricEngine.Values;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine.Nodes
{
    public class ModeSwitchTests
    {
        [Fact]
        public void canSwitchColorsByMode()
        {
            var node = new ModeSwitch("mode-colors");
            node.AddOutput("shirts", ValueType.Color);
            node.AddOutput("shorts", ValueType.Color);
            node.AddModeValues("liverpool", ("shirts", ColorRGB.RED), ("shorts", ColorRGB.RED));
            node.AddModeValues("everton", ("shirts", ColorRGB.BLUE), ("shorts", ColorRGB.WHITE));
            
            node.SetInputValue("mode", new Mode("liverpool"));

            var homePM = new MockHome(DateTime.Parse("2021-11-29T19:21:25.459551+00:00"), "sun_morning");
            var engine = new MockEngine(homePM);
            var context = new MockContext(engine);

            node.OnInit(context);
            Assert.Equal(ColorRGB.RED, node.GetOutput("shirts").value);
            Assert.Equal(ColorRGB.RED, node.GetOutput("shorts").value);

            node.SetInputValue("mode", new Mode("everton"));
            node.OnUpdate(context);
            Assert.Equal(ColorRGB.BLUE, node.GetOutput("shirts").value);
            Assert.Equal(ColorRGB.WHITE, node.GetOutput("shorts").value);
        }
   }
}