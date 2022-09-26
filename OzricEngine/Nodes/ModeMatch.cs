using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OzricEngine.Values;
using Boolean = OzricEngine.Values.Boolean;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine.Nodes;

[TypeKey(NodeType.ModeMatch)]
public class ModeMatch: Node
{
    public const string INPUT_NAME = "mode";
    public const string OUTPUT_NAME = "matches";
        
    public override NodeType nodeType => NodeType.ModeMatch;

    [JsonConverter(typeof(JsonStringEnumConverter))] 
    public enum PatternType
    {
        Exact,
        Wildcard,
        Regex
    }

    public class Pattern
    {
        public string pattern { get; set; }
        public PatternType type { get; set; }
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
        regex = ToRegex(patterns, compiled: true);

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
        var mode = GetInputValue<Mode>(INPUT_NAME).value;
        var match = regex.Any(r => r.Match(mode).Success);

        SetOutputValue(OUTPUT_NAME, new Boolean(match));
    }

    public static Regex[] ToRegex(IList<Pattern> patterns, bool compiled = false)
    {
        var options = compiled ? RegexOptions.Compiled | RegexOptions.Singleline : RegexOptions.Singleline;
        var regexes = new Regex[patterns.Count];

        int i = 0;
        foreach (var pattern in patterns)
            regexes[i++] = ToRegex(pattern, options);

        return regexes;
    }

    private static Regex ToRegex(Pattern pattern, RegexOptions options)
    {
        switch (pattern.type)
        {
            case PatternType.Regex:
                return new Regex(pattern.pattern, options);

            case PatternType.Wildcard:
                return new Regex("^" + Regex.Escape(pattern.pattern).Replace("?", ".").Replace("*", ".*") + "$", options);

            case PatternType.Exact:
                return new Regex(Regex.Escape(pattern.pattern), options);
        }

        throw new ArgumentOutOfRangeException();
    }
}