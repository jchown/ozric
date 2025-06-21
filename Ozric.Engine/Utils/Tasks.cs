using System;
using System.Threading;
using System.Threading.Tasks;
using Sentry;

namespace Ozric.Engine.Utils;

public static class Tasks
{
    public static void Run(Func<Task> function, CancellationToken cancellationToken = default)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await function();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in task: {ex.Message}: {ex.StackTrace}");
                SentrySdk.CaptureException(ex);
            }
        }, cancellationToken);
    }
}