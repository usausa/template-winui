namespace Template.WindowsApp.Services;

using Template.WindowsApp.Mocks;

public sealed class GreetServiceTests
{
    [Fact]
    public void MakeMessageAppendsTime()
    {
        // Arrange
        var timeProvider = new FixedTimeProvider(new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero));
        var service = new GreetService(timeProvider, new GreetOption { Message = "Hello" });

        // Act
        var message = service.MakeMessage();

        // Assert
        Assert.Equal("Hello (03:04:05)", message);
    }
}
