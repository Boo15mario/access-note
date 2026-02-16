using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace AccessNote.Tests;

internal static class StaTestRunner
{
    public static void Run(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        Exception? error = null;
        var thread = new Thread(
            () =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (error != null)
        {
            ExceptionDispatchInfo.Capture(error).Throw();
        }
    }
}
