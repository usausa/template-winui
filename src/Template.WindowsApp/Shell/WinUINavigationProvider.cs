namespace Template.WindowsApp.Shell;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

public sealed class WinUINavigationProvider : INavigationProvider
{
    private readonly ContainerResolver resolver;

    private readonly WinUINavigationProviderOptions options;

    public WinUINavigationProvider(ContainerResolver resolver, WinUINavigationProviderOptions options)
    {
        this.resolver = resolver;
        this.options = options;
    }

    public object? ResolveTarget(object view)
    {
        return Unsafe.As<FrameworkElement>(view).DataContext;
    }

    public void OpenView(object view)
    {
        GetContainer().Children.Add(Unsafe.As<FrameworkElement>(view));
    }

    public void CloseView(object view)
    {
        var element = Unsafe.As<FrameworkElement>(view);

        (view as IDisposable)?.Dispose();
        (element.DataContext as IDisposable)?.Dispose();
        element.DataContext = null;

        GetContainer().Children.Remove(element);
    }

    public void ActivateView(object view, object? parameter)
    {
        var element = Unsafe.As<FrameworkElement>(view);

        element.Visibility = Visibility.Visible;

        if (options.RestoreFocus && (parameter is Control focused))
        {
            focused.Focus(FocusState.Programmatic);
        }
    }

    public object? DeactivateView(object view)
    {
        var element = Unsafe.As<FrameworkElement>(view);

        var focused = options.RestoreFocus && (element.XamlRoot is { } root) ? FocusManager.GetFocusedElement(root) as DependencyObject : null;

        element.Visibility = Visibility.Collapsed;

        return (focused is not null) && IsDescendant(element, focused) ? focused : null;
    }

    private Panel GetContainer() =>
        resolver.Container ?? throw new InvalidOperationException("Container is unresolved.");

    private static bool IsDescendant(DependencyObject ancestor, DependencyObject element)
    {
        for (var current = element; current is not null; current = VisualTreeHelper.GetParent(current))
        {
            if (ReferenceEquals(current, ancestor))
            {
                return true;
            }
        }

        return false;
    }
}
