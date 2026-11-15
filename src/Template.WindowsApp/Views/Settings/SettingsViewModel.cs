namespace Template.WindowsApp.Views.Settings;

using System.Runtime.InteropServices;

using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class SettingsViewModel : AppViewModelBase
{
    private readonly IWindowManager windowManager;

    public int ThemeIndex
    {
        get => (int)windowManager.Theme;
        set => windowManager.Theme = (AppTheme)value;
    }

    public string Version { get; } = typeof(App).Assembly.GetName().Version?.ToString() ?? string.Empty;

    public string Framework { get; } = RuntimeInformation.FrameworkDescription;

    public string OperatingSystem { get; } = RuntimeInformation.OSDescription;

    public SettingsViewModel(IWindowManager windowManager)
    {
        this.windowManager = windowManager;
    }
}
