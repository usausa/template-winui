namespace Template.WindowsApp.Shell;

using Microsoft.UI.Xaml.Controls;

using Smart.Navigation.Mappers;

public static class WinUINavigatorConfigExtensions
{
    public static NavigatorConfig UseWinUINavigationProvider(this NavigatorConfig config)
    {
        return config.UseWinUINavigationProvider(static _ => { });
    }

    public static NavigatorConfig UseWinUINavigationProvider(this NavigatorConfig config, Action<WinUINavigationProviderOptions> setupAction)
    {
        var options = new WinUINavigationProviderOptions();

        setupAction(options);

        config.Configure(c =>
        {
            c.RemoveAll<ContainerResolver>();
            c.Add(new ContainerResolver());

            c.RemoveAll<ITypeConstraint>();
            c.Add<ITypeConstraint>(new AssignableTypeConstraint(typeof(Control)));

            c.Add(options);
        });

        return config.UseProvider<WinUINavigationProvider>();
    }
}
