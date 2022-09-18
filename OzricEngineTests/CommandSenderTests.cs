using OzricEngine;
using System.Collections.Generic;
using OzricEngine.Nodes;
using Xunit;
using System;

namespace OzricEngineTests
{
    public class CommandSenderTests
    {
        [Fact]
        public void canMergeCallServicesTurnOff()
        {
            var sender = new MockCommandSender();

            var turnOff1 = TurnOff("1");
            var turnOff2 = TurnOff("2");
            
            sender.Add(turnOff1, (r) => {});
            sender.Add(turnOff2, (r) => {});
            
            Assert.Single(sender.GetCommands());
            var entities = (sender.GetCommands()[0] as ClientCallService)?.target["entity_id"] as List<string> ?? throw new Exception();
            Assert.Equal(2, entities.Count);
            Assert.Contains("1", entities);
            Assert.Contains("2", entities);
        }

        [Fact]
        public void canMergeCallServicesTurnOn()
        {
            var sender = new MockCommandSender();

            var turnOff1 = TurnOn("1");
            var turnOff2 = TurnOn("2");
            
            sender.Add(turnOff1, (r) => {});
            sender.Add(turnOff2, (r) => {});
            
            Assert.Single(sender.GetCommands());
            var entities = (sender.GetCommands()[0] as ClientCallService)?.target["entity_id"] as List<string> ?? throw new Exception();
            Assert.Equal(2, entities.Count);
            Assert.Contains("1", entities);
            Assert.Contains("2", entities);
        }

        private ClientCallService TurnOff(string id)
        {
            return new ClientCallService
            {
                domain = "light",
                service = "turn_off",
                target = new Attributes()
                {
                    { "entity_id", new List<string> { id } }
                },
            };
        }

        private ClientCallService TurnOn(string id)
        {
            return new ClientCallService
            {
                domain = "light",
                service = "turn_on",
                target = new Attributes
                {
                    { "entity_id", new List<string> { id } }
                },
                service_data = new Attributes
                {
                    { "brightness", 100},
                    { "rgb_color", "100,100,100" }
                },
            };
        }
    }
}
