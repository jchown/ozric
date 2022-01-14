using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using OzricEngine;
using OzricEngine.ext;

namespace OzricEngineTests
{
    public class MockEngine: Engine
    {
        public MockEngine() : this(new MockHome(new List<EntityState>()))
        {
        }
        
        public MockEngine(MockHome home) : this(home, new Graph())
        {
        }
        
        public MockEngine(MockHome home, Graph graph) : base(home, graph, new MockComms())
        {
        }

        public void ProcessMockEvent(string name)
        {
            var json = File.ReadAllText($"../../../events/{name}.json");
            try
            {
                 var ev = Json.Deserialize<ServerEvent>(json);
                 ProcessEvents( new List<ServerEvent> { ev });
            }
            catch (Exception e)
            {
                throw e.Rethrown($"while parsing states/{name}.json");
            }
        }
    }
}