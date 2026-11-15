namespace Template.WindowsApp.Components;

using Microsoft.UI.Xaml.Controls;

using Template.WindowsApp.Shell;

public sealed partial class Dialog : IDialog, IDisposable
{
    private readonly SemaphoreSlim semaphore = new(1, 1);

    private readonly DialogHost host;

    public Dialog(DialogHost host)
    {
        this.host = host;
    }

    public void Dispose()
    {
        semaphore.Dispose();
    }

    public Task<bool> ConfirmAsync(string message) =>
        ConfirmAsync("Confirm", message, "OK", "Cancel");

    public async Task<bool> ConfirmAsync(string title, string message, string acceptText, string cancelText) =>
        await ShowAsync(title, message, acceptText, cancelText).ConfigureAwait(true) == ContentDialogResult.Primary;

    public Task NotifyAsync(string message) =>
        NotifyAsync("Information", message);

    public Task NotifyAsync(string title, string message) =>
        ShowAsync(title, message, null, "OK");

    private async Task<ContentDialogResult> ShowAsync(string title, string message, string? primaryText, string closeText)
    {
        await semaphore.WaitAsync().ConfigureAwait(true);
        try
        {
            if (host.XamlRoot is not { } root)
            {
                return ContentDialogResult.None;
            }

            var dialog = new ContentDialog
            {
                XamlRoot = root,
                Style = (Style)Application.Current.Resources["DefaultContentDialogStyle"],
                Title = title,
                Content = new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    IsTextSelectionEnabled = true
                },
                CloseButtonText = closeText,
                DefaultButton = primaryText is null ? ContentDialogButton.Close : ContentDialogButton.Primary
            };
            if (primaryText is not null)
            {
                dialog.PrimaryButtonText = primaryText;
            }

            return await dialog.ShowAsync();
        }
        finally
        {
            semaphore.Release();
        }
    }
}
