using OzricEngine;

namespace OzricService.Model;

public class Options
{
    public string token { get; set; }
    public string logging { get; set; }

    public static Options Instance { get; }

    static Options()
    {
        var json = File.ReadAllText("/data/options.json");

        Instance = Json.Deserialize<Options>(json);
    }
}