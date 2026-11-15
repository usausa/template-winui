namespace Template.WindowsApp.Components;

public interface IDialog
{
    Task<bool> ConfirmAsync(string message);

    Task<bool> ConfirmAsync(string title, string message, string acceptText, string cancelText);

    Task NotifyAsync(string message);

    Task NotifyAsync(string title, string message);
}
