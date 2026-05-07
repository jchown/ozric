namespace Ozric.Service;

/// <summary>
/// Use a Git repository to store & version data.
/// </summary>
public static class Storage
{
    public static readonly string RootPath = GetRootPath();

    public static void Commit(string commitMessage)
    {
        GitExec("add .");
        GitExec($"commit -m \"{commitMessage}\"");  // TODO: Escape commit message properly
    }

    static Storage()
    {
        if (!Directory.Exists(RootPath))
        {
            Directory.CreateDirectory(RootPath);
        }
        
        if (!Directory.Exists($"{RootPath}/.git"))
        {
            GitExec("init");
        }
    }

    private static void GitExec(string arguments)
    {
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            WorkingDirectory = RootPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = System.Diagnostics.Process.Start(startInfo))
        {
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start git process.");
            }
            
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                throw new InvalidOperationException($"Failed to commit changes: {error}");
            }
        }
    }

    private static string GetRootPath()
    {
        //  HA Supervisor injects SUPERVISOR_TOKEN when running as an add-on; that's
        //  also where /data is mounted as the persistent volume. Without it (local
        //  dev on macOS, or a Linux devcontainer) keep state under the user profile.

        if (Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN") != null)
            return "/data";

        return $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.ozric/data";
    }

    public static readonly string GraphFilename = RootPath + "/graph.json";
}