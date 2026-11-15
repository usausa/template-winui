namespace Template.WindowsApp;

using System.Runtime.InteropServices;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppLifecycle;

using Smart.Mvvm.Resolver;
using Smart.Threading;

using Template.WindowsApp.Components;
using Template.WindowsApp.Shell;
using Template.WindowsApp.Views;

public sealed partial class App
{
    private readonly DispatcherQueue dispatcherQueue;

    private readonly IHost host;

    private readonly ILogger<App> log;

    private readonly IWindowManager windowManager;

    private IDialog? dialog;

    private bool showingError;

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public App()
    {
        InitializeComponent();

        DispatcherShutdownMode = DispatcherShutdownMode.OnExplicitShutdown;
        dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        SynchronizationContext.SetSynchronizationContext(new ExceptionHandlingSynchronizationContext(dispatcherQueue, HandleException));

        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        host = CreateHost();

        log = host.Services.GetRequiredService<ILogger<App>>();
        windowManager = host.Services.GetRequiredService<IWindowManager>();
        ResolveProvider.Default.Provider = host.Services;

        var environment = host.Services.GetRequiredService<IHostEnvironment>();
        ThreadPool.GetMinThreads(out var workerThreads, out var completionPortThreads);

        log.InfoStartup();
        log.InfoStartupSettingsRuntime(RuntimeInformation.OSDescription, RuntimeInformation.FrameworkDescription, RuntimeInformation.RuntimeIdentifier);
        log.InfoStartupSettingsGC(GCSettings.IsServerGC, GCSettings.LatencyMode, GCSettings.LargeObjectHeapCompactionMode);
        log.InfoStartupSettingsThreadPool(workerThreads, completionPortThreads);
        log.InfoStartupApplication(environment.ApplicationName, typeof(App).Assembly.GetName().Version);
        log.InfoStartupEnvironment(environment.EnvironmentName, environment.ContentRootPath);

        UnhandledException += (_, ea) => ea.Handled = HandleException(ea.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, ea) => log.ErrorUnknownException((Exception)ea.ExceptionObject);
        TaskScheduler.UnobservedTaskException += (_, ea) => log.ErrorUnobservedTaskException(ea.Exception);
        AppInstance.GetCurrent().Activated += OnActivated;
    }

    private static IHost CreateHost()
    {
        var builder = Host.CreateApplicationBuilder(Environment.GetCommandLineArgs());

        // Container
        builder.ConfigureContainer();
        // Log
        builder.ConfigureLogging();
        // Components
        builder.ConfigureComponents();

        var host = builder.Build();
#if DEBUG
        if (host.Services is BunnyTail.DependencyInjection.GeneratedServiceProvider generatedProvider)
        {
            foreach (var line in BunnyTail.DependencyInjection.Diagnostics.ServiceFactoryReportExtensions.DescribeRuntimeFallbacks(generatedProvider).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
            {
                System.Diagnostics.Debug.WriteLine(line);
            }
        }

        // Setup navigator
        var navigator = host.Services.GetRequiredService<INavigator>();
        navigator.Navigated += (_, args) =>
        {
            // for debug
            System.Diagnostics.Debug.WriteLine($"Navigated: [{args.Context.FromId}]->[{args.Context.ToId}] : stacked=[{navigator.StackedCount}]");
        };
#endif
        return host;
    }

    //--------------------------------------------------------------------------------
    // Lifecycle
    //--------------------------------------------------------------------------------

    // ReSharper disable once AsyncVoidMethod
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        var window = windowManager.Load();
        host.Services.GetRequiredService<DialogHost>().Attach(window);
        dialog = host.Services.GetRequiredService<IDialog>();
        window.Closed += OnMainWindowClosed;
        window.Activate();

        await host.StartAsync().ConfigureAwait(true);

        await host.Services.GetRequiredService<INavigator>().ForwardAsync(ViewId.Home).ConfigureAwait(true);
    }

    // ReSharper disable once AsyncVoidMethod
    private async void OnMainWindowClosed(object sender, WindowEventArgs args)
    {
        windowManager.Save();
        dialog = null;
        try
        {
            await host.StopAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(true);
        }
        finally
        {
            host.Dispose();
            Exit();
        }
    }

    //--------------------------------------------------------------------------------
    // Instance
    //--------------------------------------------------------------------------------

    private void OnActivated(object? sender, AppActivationArguments args)
    {
        _ = dispatcherQueue.TryEnqueue(() => OnSecondInstance(args));
    }

    private void OnSecondInstance(AppActivationArguments args)
    {
        log.InfoSecondInstance(args.Data is Windows.ApplicationModel.Activation.ILaunchActivatedEventArgs launch ? launch.Arguments : args.Kind.ToString());
        windowManager.Activate();
    }

    //--------------------------------------------------------------------------------
    // Error
    //--------------------------------------------------------------------------------

    private bool HandleException(Exception ex)
    {
        log.ErrorUnknownException(ex);
        ShowError(ex);
        return true;
    }

    // ReSharper disable once AsyncVoidMethod
    private async void ShowError(Exception ex)
    {
        if (showingError || (dialog is null))
        {
            return;
        }

        showingError = true;
        await dialog.NotifyAsync("Error", ex.Message).ConfigureAwait(true);
        showingError = false;
    }
}
