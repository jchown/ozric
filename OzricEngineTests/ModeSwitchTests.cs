using System;
using OzricEngineTests;
using Xunit;

namespace OzricEngine.logic
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
            Assert.Equal(ColorRGB.RED, node.GetOutputValue("shirts"));
            Assert.Equal(ColorRGB.RED, node.GetOutputValue("shorts"));

            node.SetInputValue("mode", new Mode("everton"));
            node.OnUpdate(context);
            Assert.Equal(ColorRGB.BLUE, node.GetOutputValue("shirts"));
            Assert.Equal(ColorRGB.WHITE, node.GetOutputValue("shorts"));
        }
   }
}