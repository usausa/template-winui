namespace Template.WindowsApp;

using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppLifecycle;

using Template.WindowsApp.Helpers;

public static class Program
{
    private static readonly TimeSpan RedirectTimeout = TimeSpan.FromSeconds(5);

    [STAThread]
    public static void Main()
    {
        WinRT.ComWrappersSupport.InitializeComWrappers();

        var instance = AppInstance.FindOrRegisterForKey(typeof(Program).Assembly.GetName().Name);
        if (!instance.IsCurrent)
        {
            Redirect(instance);
            return;
        }

        Application.Start(static p =>
        {
            SynchronizationContext.SetSynchronizationContext(new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread()));
            _ = new App();
        });
    }

    private static void Redirect(AppInstance instance)
    {
        _ = NativeMethods.AllowSetForegroundWindow(instance.ProcessId);

        var args = AppInstance.GetCurrent().GetActivatedEventArgs();
        _ = Task.Run(() => instance.RedirectActivationToAsync(args).AsTask()).Wait(RedirectTimeout);
    }
}
