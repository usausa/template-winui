namespace Template.WindowsApp.Views.Home;

using Template.WindowsApp.Components;
using Template.WindowsApp.Services;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class HomeViewModel : AppViewModelBase
{
    public IObserveCommand GreetCommand { get; }

    public HomeViewModel(IDialog dialog, GreetService greetService)
    {
        GreetCommand = MakeAsyncCommand(() => dialog.NotifyAsync(greetService.MakeMessage()));
    }
}
