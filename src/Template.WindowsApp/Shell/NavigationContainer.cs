namespace Template.WindowsApp.Shell;

using Microsoft.UI.Xaml.Controls;

public static class NavigationContainer
{
    public static readonly DependencyProperty NavigatorProperty = DependencyProperty.RegisterAttached(
        "Navigator",
        typeof(INavigator),
        typeof(NavigationContainer),
        new PropertyMetadata(null, HandlePropertyChanged));

    public static INavigator GetNavigator(DependencyObject obj) =>
        (INavigator)obj.GetValue(NavigatorProperty);

    public static void SetNavigator(DependencyObject obj, INavigator value) =>
        obj.SetValue(NavigatorProperty, value);

    private static void HandlePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if ((d is Panel panel) && (e.NewValue is INavigatorComponentSource componentSource))
        {
            componentSource.Components.Get<ContainerResolver>().Attach(panel);
        }
    }
}
