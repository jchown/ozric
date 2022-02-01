using Microsoft.AspNetCore.Rewrite;

public static class Paths
{
    private static readonly Dictionary<string, string> pagePaths = new()
    {
        { "/", "/status.html" },
        { "/status", "/status.html" },
        { "/graph", "/graph.html" }
    };
    
    public static void RewritePagePaths(RewriteContext context)
    {
        var request = context.HttpContext.Request;
        var path = request.Path.Value;

        if (pagePaths.ContainsKey(path))
        {
            var redirection = pagePaths[path];
            Console.WriteLine($"Path: {path} => {redirection}");

            context.Result = RuleResult.SkipRemainingRules;
            request.Path = redirection;
        }
    }
}