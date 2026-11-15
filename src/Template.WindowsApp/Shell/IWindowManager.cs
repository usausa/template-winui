namespace Template.WindowsApp.Shell;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage;

public enum AppTheme
{
    Light,
    Dark,
    System
}

public interface IWindowManager
{
    AppTheme Theme { get; set; }

    MainWindow Load();

    void Save();

    void Activate();
}

public sealed partial class WindowManager : IWindowManager, IDisposable
{
    private const string ThemeKey = "Theme";

    private const string PaneOpenKey = "IsPaneOpen";

    private readonly IServiceProvider provider;

    private readonly ApplicationData data;

    private MainWindow? mainWindow;

    public AppTheme Theme
    {
        get;
        set
        {
            field = value;
            ApplyTheme();
        }
    }

    public WindowManager(IServiceProvider provider)
    {
        this.provider = provider;

        var name = typeof(WindowManager).Assembly.GetName().Name;
        data = ApplicationData.GetForUnpackaged(name, name);
        WinUIEx.WindowManager.PersistenceStorage = data.LocalSettings.Values;
    }

    public void Dispose()
    {
        data.Dispose();
    }

    public MainWindow Load()
    {
        var window = provider.GetRequiredService<MainWindow>();
        mainWindow = window;

        var values = data.LocalSettings.Values;
        window.IsPaneOpen = !values.TryGetValue(PaneOpenKey, out var paneOpen) || (paneOpen is true);
        Theme = values.TryGetValue(ThemeKey, out var theme) && Enum.TryParse<AppTheme>(theme as string, out var value) ? value : AppTheme.System;

        var menu = CreateTrayMenu(window);
        var manager = WinUIEx.WindowManager.Get(window);
        manager.PersistenceId = nameof(MainWindow);
        manager.IsVisibleInTray = true;
        manager.TrayIconSelected += (_, e) =>
        {
            e.Handled = true;
            Activate();
        };
        manager.TrayIconContextMenu += (_, e) => e.Flyout = menu;

        return window;
    }

    public void Save()
    {
        if (mainWindow is not { } window)
        {
            return;
        }

        var values = data.LocalSettings.Values;
        values[PaneOpenKey] = window.IsPaneOpen;
        values[ThemeKey] = Theme.ToString();

        mainWindow = null;
    }

    public void Activate()
    {
        if (mainWindow is not { } window)
        {
            return;
        }

        if (!window.AppWindow.IsVisible)
        {
            window.AppWindow.Show(true);
        }
        if (window.AppWindow.Presenter is OverlappedPresenter { State: OverlappedPresenterState.Minimized } presenter)
        {
            presenter.Restore(true);
        }
        window.Activate();
    }

    private void ApplyTheme()
    {
        if (mainWindow is not { } window)
        {
            return;
        }

        if (window.Content is FrameworkElement root)
        {
            root.RequestedTheme = Theme switch
            {
                AppTheme.Light => ElementTheme.Light,
                AppTheme.Dark => ElementTheme.Dark,
                _ => ElementTheme.Default
            };
        }
        window.AppWindow.TitleBar.PreferredTheme = Theme switch
        {
            AppTheme.Light => TitleBarTheme.Light,
            AppTheme.Dark => TitleBarTheme.Dark,
            _ => TitleBarTheme.UseDefaultAppMode
        };
    }

    private MenuFlyout CreateTrayMenu(MainWindow window)
    {
        var open = new MenuFlyoutItem { Text = "Open" };
        open.Click += (_, _) => Activate();
        var exit = new MenuFlyoutItem { Text = "Exit" };
        exit.Click += (_, _) => window.Close();

        var menu = new MenuFlyout();
        menu.Items.Add(open);
        menu.Items.Add(exit);
        return menu;
    }
}
