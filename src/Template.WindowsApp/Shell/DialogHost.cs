namespace Template.WindowsApp.Shell;

public sealed class DialogHost
{
    private Window? window;

    public XamlRoot? XamlRoot => window?.Content?.XamlRoot;

    public void Attach(Window value)
    {
        window = value;
    }
}
