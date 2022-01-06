using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using OzricEngine;
using OzricEngine.ext;

namespace OzricEngineTests
{
    public class MockEngine: Engine
    {
        public MockEngine() : base(new MockHome(new List<State>()), new MockComms())
        {
        }
        
        public MockEngine(MockHome home) : base(home, new MockComms())
        {
        }

        public async Task ProcessMockEvent(string name)
        {
            var json = File.ReadAllText($"../../../events/{name}.json");
            try
            {
                 var ev = JsonSerializer.Deserialize<ServerEvent>(json, Comms.JsonOptions);
                 await ProcessEvents( new List<ServerEvent> { ev });
            }
            catch (Exception e)
            {
                throw e.Rethrown($"while parsing states/{name}.json");
            }
        }
    }
}