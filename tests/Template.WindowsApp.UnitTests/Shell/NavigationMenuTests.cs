namespace Template.WindowsApp.Shell;

using Template.WindowsApp.Views;

public sealed class NavigationMenuTests
{
    [Fact]
    public void FindReturnsItemOfView()
    {
        // Arrange
        var menu = new NavigationMenu();

        // Act
        var home = menu.Find(ViewId.Home);
        var settings = menu.Find(ViewId.Settings);

        // Assert
        Assert.Same(menu.Items[0], home);
        Assert.Same(menu.FooterItems[0], settings);
    }

    [Fact]
    public void ItemsContainEachViewOnce()
    {
        // Arrange
        var menu = new NavigationMenu();

        // Act
        var ids = menu.Items.Concat(menu.FooterItems).Select(static x => x.Id).ToList();

        // Assert
        Assert.Equal(Enum.GetValues<ViewId>().Order(), ids.Order());
    }
}
