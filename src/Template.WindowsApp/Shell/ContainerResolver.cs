namespace Template.WindowsApp.Shell;

using Microsoft.UI.Xaml.Controls;

public sealed class ContainerResolver
{
    public Panel? Container { get; private set; }

    public void Attach(Panel? container)
    {
        Container = container;
    }
}
