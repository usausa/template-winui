namespace Template.WindowsApp.Shell;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.System;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.Storage;

using Windows.UI;

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

    private ThemeSettings? themeSettings;

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
        if (window.Content is FrameworkElement root)
        {
            root.ActualThemeChanged += (_, _) => UpdateCaptionButtons(window);
        }
        themeSettings = ThemeSettings.CreateForWindowId(window.AppWindow.Id);
        themeSettings.Changed += (_, _) => window.DispatcherQueue.TryEnqueue(() => UpdateCaptionButtons(window));

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
        UpdateCaptionButtons(window);
    }

    private void UpdateCaptionButtons(MainWindow window)
    {
        var titleBar = window.AppWindow.TitleBar;
        if (themeSettings is { HighContrast: true })
        {
            titleBar.ButtonBackgroundColor = null;
            titleBar.ButtonInactiveBackgroundColor = null;
            titleBar.ButtonHoverBackgroundColor = null;
            titleBar.ButtonPressedBackgroundColor = null;
            titleBar.ButtonForegroundColor = null;
            titleBar.ButtonHoverForegroundColor = null;
            titleBar.ButtonPressedForegroundColor = null;
            titleBar.ButtonInactiveForegroundColor = null;
            return;
        }

        var dark = window.Content is FrameworkElement { ActualTheme: ElementTheme.Dark };
        var baseColor = dark ? Colors.White : Colors.Black;
        var background = CaptionColor("WindowCaptionButtonBackground", baseColor);
        var foreground = CaptionColor("WindowCaptionForeground", baseColor);
        titleBar.ButtonBackgroundColor = background;
        titleBar.ButtonInactiveBackgroundColor = background;
        titleBar.ButtonHoverBackgroundColor = CaptionColor("WindowCaptionButtonBackgroundPointerOver", baseColor);
        titleBar.ButtonPressedBackgroundColor = CaptionColor("WindowCaptionButtonBackgroundPressed", baseColor);
        titleBar.ButtonForegroundColor = foreground;
        titleBar.ButtonHoverForegroundColor = foreground;
        titleBar.ButtonPressedForegroundColor = foreground;
        titleBar.ButtonInactiveForegroundColor = Compose(CaptionColor("WindowCaptionForegroundDisabled", baseColor), ThemeColor("SolidBackgroundFillColorBase", dark));
    }

    private static Color CaptionColor(string key, Color baseColor)
    {
        var alpha = Application.Current.Resources.TryGetValue(key, out var value) && (value is SolidColorBrush brush) ? brush.Color.A : byte.MaxValue;
        return Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B);
    }

    private static Color ThemeColor(string key, bool dark)
    {
        var theme = dark ? "Default" : "Light";
        foreach (var dictionary in Application.Current.Resources.MergedDictionaries)
        {
            if (dictionary.ThemeDictionaries.TryGetValue(theme, out var value) && (value is ResourceDictionary resources) && resources.TryGetValue(key, out var color) && (color is Color result))
            {
                return result;
            }
        }
        return Colors.Transparent;
    }

    private static Color Compose(Color foreground, Color background)
    {
        var alpha = foreground.A / 255d;
        return Color.FromArgb(byte.MaxValue, Mix(foreground.R, background.R, alpha), Mix(foreground.G, background.G, alpha), Mix(foreground.B, background.B, alpha));
    }

    private static byte Mix(byte foreground, byte background, double alpha) => (byte)Math.Round(background + ((foreground - background) * alpha));

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
