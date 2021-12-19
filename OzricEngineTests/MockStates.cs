using System;
using System.IO;
using System.Text.Json;
using OzricEngine;
using OzricEngine.ext;

namespace OzricEngineTests
{
    public static class MockStates
    {
        public static State Load(string name)
        {
            var json = File.ReadAllText($"../../../states/{name}.json");
            try
            {
                return JsonSerializer.Deserialize<State>(json, Comms.JsonOptions);
            }
            catch (Exception e)
            {
                throw e.Rethrown($"while parsing states/{name}.json");
            }
        }
    }
}