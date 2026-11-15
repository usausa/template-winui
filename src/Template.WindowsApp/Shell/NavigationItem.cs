namespace Template.WindowsApp.Shell;

using Template.WindowsApp.Views;

public sealed class NavigationItem
{
    public ViewId Id { get; }

    public string Title { get; }

    public string Glyph { get; }

    public NavigationItem(ViewId id, string title, string glyph)
    {
        Id = id;
        Title = title;
        Glyph = glyph;
    }
}
