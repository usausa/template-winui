namespace Template.WindowsApp.Views.Home;

[View(ViewId.Home)]
public sealed partial class HomeView
{
    public HomeViewModel ViewModel => (HomeViewModel)DataContext;

    public HomeView()
    {
        InitializeComponent();
    }
}
