using OzricEngine;

namespace OzricService.Model;

public class Options
{
    private const string FILENAME = "/data/options.json";

    public string logging { get; set; } = "Info";

    public static Options Instance { get; }

    static Options()
    {
        var options = new Options(); 
        
        if (File.Exists(FILENAME))
        {
            var json = File.ReadAllText(FILENAME);
            try
            {
                options = Json.Deserialize<Options>(json);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to load options: {e.Message}");
            }
        }

        Instance = options;
    }
}