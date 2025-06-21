using System.Runtime.InteropServices;

namespace OzricService;

public static class Storage
{
    public static readonly string RootPath = GetRootPath();

    private static string GetRootPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Assume we are running inside the container.

            return "/data";
        }

        //  Outside the container, use ~/.ozric/data

        return $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.ozric/data";
    }

    public static readonly string GraphFilename = RootPath + "/graph.json";
}