using System;
using System.Collections.Generic;
using System.IO;
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

        public ServerEvent LoadMockEvent(string name)
        {
            var json = File.ReadAllText($"../../../events/{name}.json");
            try
            {
                return Json.Deserialize<ServerEvent>(json);
            }
            catch (Exception e)
            {
                throw e.Rethrown($"while parsing events/{name}.json");
            }
        }

        public bool ProcessMockEvent(string name)
        {
            return ProcessEvents(new() { LoadMockEvent(name) });
        }
    }
}