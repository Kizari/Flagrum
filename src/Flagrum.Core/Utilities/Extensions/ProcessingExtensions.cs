using System.Collections.Generic;
using System.Threading;

namespace Flagrum.Core.Utilities.Extensions;

public static class ProcessingExtensions
{
    public static void WaitAll(this IEnumerable<Thread> threads)
    {
        foreach (var thread in threads)
        {
            thread.Join();
        }
    }
}