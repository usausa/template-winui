namespace Template.WindowsApp.Shell;

using Template.WindowsApp.Views;

public sealed class NavigationMenu
{
    public IReadOnlyList<NavigationItem> Items { get; } = (NavigationItem[])[
        new(ViewId.Home, "Home", "\uE80F")
    ];

    public IReadOnlyList<NavigationItem> FooterItems { get; } = (NavigationItem[])[
        new(ViewId.Settings, "Settings", "\uE713")
    ];

    public NavigationItem? Find(ViewId id)
    {
        foreach (var item in Items)
        {
            if (item.Id == id)
            {
                return item;
            }
        }

        foreach (var item in FooterItems)
        {
            if (item.Id == id)
            {
                return item;
            }
        }

        return null;
    }
}
