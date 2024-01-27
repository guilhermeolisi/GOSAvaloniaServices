using Avalonia;
using Avalonia.Controls;

namespace GOSBaseInjection;

public class CopyTextClipboard : ICopyTextClipboard
{
    //https://docs.avaloniaui.net/docs/concepts/services/clipboard
    TopLevel topLevel;
    public void SetTopLevel(object topLevel)
    {
        if (topLevel is null)
            throw new ArgumentNullException(nameof(topLevel));
        if (topLevel is not TopLevel)
            throw new ArgumentException("topLevel is not TopLevel", nameof(topLevel));
        this.topLevel = topLevel as TopLevel;
    }
    public async Task<string?> CopyFromClipBoard()
    {
        //return await Application.Current.Clipboard.GetTextAsync();
        return await topLevel!.Clipboard!.GetTextAsync();

    }
    public async Task CopyToClipBoard(string message)
    {
        if (message is not null)
            await topLevel!.Clipboard!.SetTextAsync(message);
            //await Application.Current.Clipboard.SetTextAsync(message);
    }
}
