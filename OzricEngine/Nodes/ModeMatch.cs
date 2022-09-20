using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OzricEngine.Values;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine.Nodes;

[TypeKey(NodeType.ModeMatch)]
public class ModeMatch: Node
{
    public const string INPUT_NAME = "mode";
    public const string OUTPUT_NAME = "matches";
        
    public override NodeType nodeType => NodeType.ModeMatch;

    public struct Pattern
    {
        public string pattern { get; set; }
        public bool regex { get; set; }
    };

    public Pattern[] patterns { get; set; }

    [JsonIgnore]
    public Regex[] regex; 

    public ModeMatch() : this(null)
    {
    }

    public ModeMatch(string id): base(id, new List<Pin> { new(INPUT_NAME, ValueType.Mode) }, new List<Pin> { new(OUTPUT_NAME, ValueType.Boolean) })
    {
        patterns = new Pattern[0];
        regex = new Regex[0];
    }

    public override Task OnInit(Context context)
    {
        regex = Compile(patterns);

        UpdateValue(context);
        return Task.CompletedTask;
    }

    public override Task OnUpdate(Context context)
    {
        UpdateValue(context);
        return Task.CompletedTask;
    }

    private void UpdateValue(Context engine)
    {
        var mode = GetInputMode(INPUT_NAME).value;
        var match = regex.Any(r => r.Match(mode).Success);

        SetOutputValue(OUTPUT_NAME, new Boolean(match));
    }

    public static Regex[] Compile(IList<Pattern> patterns)
    {
        var compiled = new Regex[patterns.Count];
        int i = 0;
        foreach (var pattern in patterns)
        {
            if (pattern.regex)
                compiled[i++] = new Regex(pattern.pattern, RegexOptions.Compiled | RegexOptions.Singleline);
            else
                compiled[i++] = new Regex(Regex.Escape(pattern.pattern).Replace("?", ".").Replace("*", ".*"), RegexOptions.Compiled | RegexOptions.Singleline);
        }

        return compiled;
    }
}