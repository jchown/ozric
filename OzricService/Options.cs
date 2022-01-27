using OzricEngine;

namespace OzricService.Model;

public class Options
{
    private const string FILENAME = "/data/options.json";
    private const string ENV_VAR = "OZRIC_HA_TOKEN";

    public string token { get; set; } = "<not set>";
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
        else
        {
            Console.WriteLine($"No options file, looking for token in env. var {ENV_VAR} ");
            
            options.token = Environment.GetEnvironmentVariable(ENV_VAR) ?? throw new Exception("No token");
        }

        Instance = options;
    }
}