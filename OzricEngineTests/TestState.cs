using System.IO;
using System.Text.Json;
using OzricEngine;

namespace OzricEngineTests
{
    public static class TestState
    {
        public static State Load(string name)
        {
            var json = File.ReadAllText($"../../../{name}.json");
            return JsonSerializer.Deserialize<State>(json, Connection.JsonOptions);
        }
    }
}