using System;
using System.Threading;
using System.Threading.Tasks;

namespace Flagrum.Core.Utilities;

public static class ThreadHelper
{
    public static void RunOnNewThread(Func<Task> task)
    {
        new Thread(() => task().Wait()).Start();
    }
}