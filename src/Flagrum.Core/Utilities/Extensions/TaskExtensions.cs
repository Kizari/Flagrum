using System.Threading.Tasks;

namespace Flagrum.Core.Utilities.Extensions;

public static class TaskExtensions
{
    public static void AwaitSynchronous(this Task task)
    {
        task.ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static TResult AwaitSynchronous<TResult>(this Task<TResult> task)
    {
        return task.ConfigureAwait(false).GetAwaiter().GetResult();
    }
    
    public static void AwaitSynchronous(this ValueTask task)
    {
        task.ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static TResult AwaitSynchronous<TResult>(this ValueTask<TResult> task)
    {
        return task.ConfigureAwait(false).GetAwaiter().GetResult();
    }
}