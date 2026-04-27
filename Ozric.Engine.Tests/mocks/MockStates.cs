using System;
using System.IO;
using Ozric.Engine.Extensions;
using Ozric.Engine.Model;

namespace Ozric.Engine.Tests
{
    public static class MockStates
    {
        public static EntityState Load(string name)
        {
            var json = File.ReadAllText($"../../../states/{name}.json");
            try
            {
                return Json.Deserialize<EntityState>(json);
            }
            catch (Exception e)
            {
                throw e.Rethrown($"while parsing states/{name}.json");
            }
        }
    }
}