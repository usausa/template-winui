namespace Template.WindowsApp.Views.Settings;

[View(ViewId.Settings)]
public sealed partial class SettingsView
{
    public SettingsViewModel ViewModel => (SettingsViewModel)DataContext;

    public SettingsView()
    {
        InitializeComponent();
    }
}
