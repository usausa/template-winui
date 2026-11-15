namespace Template.WindowsApp.Views.Settings;

using System.Runtime.InteropServices;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class SettingsViewModel : AppViewModelBase
{
    public string Version { get; } = typeof(App).Assembly.GetName().Version?.ToString() ?? string.Empty;

    public string Framework { get; } = RuntimeInformation.FrameworkDescription;

    public string OperatingSystem { get; } = RuntimeInformation.OSDescription;
}
