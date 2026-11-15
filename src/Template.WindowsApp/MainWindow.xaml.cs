namespace Template.WindowsApp;

using Microsoft.UI.Windowing;

using Smart.WinUI.Interactivity;

using Template.WindowsApp.Helpers;

using Windows.Graphics;

using WinRT.Interop;

public sealed partial class MainWindow
{
    private const int DefaultWidth = 1280;

    private const int DefaultHeight = 960;

    private const int MinimumWidth = 1024;

    private const int MinimumHeight = 768;

    public MainWindowViewModel ViewModel { get; }

    public MainWindow(MainWindowViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "App.ico"));

        var scale = NativeMethods.GetDpiForWindow(WindowNative.GetWindowHandle(this)) / 96d;
        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = (int)(MinimumWidth * scale);
            presenter.PreferredMinimumHeight = (int)(MinimumHeight * scale);
        }

        AppWindow.Resize(new SizeInt32((int)(DefaultWidth * scale), (int)(DefaultHeight * scale)));
        WindowPlacementHelper.UpdatePlacement(AppWindow, DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest).WorkArea, WindowPlacement.Center, scale: scale);
    }
}
