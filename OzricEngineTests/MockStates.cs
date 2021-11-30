using System.IO;
using System.Text.Json;
using OzricEngine;

namespace OzricEngineTests
{
    public static class MockStates
    {
        public static State Load(string name)
        {
            var json = File.ReadAllText($"../../../states/{name}.json");
            return JsonSerializer.Deserialize<State>(json, Comms.JsonOptions);
        }
    }
}