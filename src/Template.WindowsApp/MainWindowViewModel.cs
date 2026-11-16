namespace Template.WindowsApp;

using Template.WindowsApp.Extender;
using Template.WindowsApp.Shell;
using Template.WindowsApp.Views;

// ReSharper disable once ClassNeverInstantiated.Global
[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public sealed partial class MainWindowViewModel : ExtendViewModelBase
{
    public INavigator Navigator { get; }

    public NavigationMenu Menu { get; }

    [ObservableProperty]
    public partial NavigationItem? SelectedItem { get; set; }

    public MainWindowViewModel(INavigator navigator, NavigationMenu menu)
    {
        Navigator = navigator;
        Menu = menu;

        Disposables.Add(Observable
            .FromEventPattern<NavigationEventArgs>(h => navigator.Navigated += h, h => navigator.Navigated -= h)
            .Subscribe(x => SelectedItem = x.EventArgs.Context.ToId is ViewId id ? menu.Find(id) : null));
        Disposables.Add(ObserveSelectedItem()
            .Where(x => (x is not null) && !Equals(navigator.CurrentViewId, x.Id))
            .Select(x => Observable.FromAsync(() => navigator.ForwardAsync(x!.Id, new NavigationParameter().WithEffect(EntranceEffect.Name))))
            .Concat()
            .Subscribe());
    }
}
