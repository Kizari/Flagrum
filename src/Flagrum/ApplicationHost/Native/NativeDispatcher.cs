using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Flagrum.ApplicationHost.Native;

/// <summary>
/// C# wrapper for the NativeDispatcher class.
/// </summary>
public sealed partial class NativeDispatcher : IDisposable
{
    private readonly IntPtr _instance = NativeDispatcher_Create();

    /// <inheritdoc />
    public void Dispose()
    {
        NativeDispatcher_Destroy(_instance);
    }

    /// <summary>
    /// Invokes an action on the native UI thread.
    /// </summary>
    /// <param name="callback">Action to invoke.</param>
    public void Invoke(Action callback)
    {
        var completion = new ManualResetEventSlim(false);

        NativeDispatcher_Invoke(_instance, () =>
        {
            try
            {
                callback();
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Exception occurred during main thread invocation");
                Debug.WriteLine(exception);
            }
            finally
            {
                completion.Set();
            }
        });

        completion.Wait();
    }


    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial IntPtr NativeDispatcher_Create();

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial void NativeDispatcher_Destroy(IntPtr instance);

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial void NativeDispatcher_Invoke(IntPtr instance, Action callback);
}