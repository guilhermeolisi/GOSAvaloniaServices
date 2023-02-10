using Avalonia;

namespace GOSBaseInjection;

public class CopyTextClipboard : ICopyTextClipboard
{
    public async Task<string?> CopyFromClipBoard()
    {
        return await Application.Current.Clipboard.GetTextAsync();
    }
    public async Task CopyToClipBoard(string message)
    {
        if (message is not null)
            await Application.Current.Clipboard.SetTextAsync(message);
    }
}
