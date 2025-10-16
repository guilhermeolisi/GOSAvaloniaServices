using Avalonia.Controls;
using Avalonia.Input.Platform;

namespace GOSBaseInjection;

public class CopyTextClipboard : ICopyTextClipboard
{
    private SynchronizationContext uiContext;
    public bool IsOnUIThread => uiContext == SynchronizationContext.Current;
    public CopyTextClipboard()
    {
        uiContext = SynchronizationContext.Current ?? throw new InvalidOperationException("UI thread context is not available.");
    }
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
        string? result;
        if (IsOnUIThread)
        {
            result = await topLevel!.Clipboard!.TryGetTextAsync();
        }
        else
        {
            var tcs = new TaskCompletionSource<string>();
            uiContext?.Post(async _ =>
            {
                try
                {
                    var text = await topLevel!.Clipboard!.TryGetTextAsync();
                    tcs.SetResult(text);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, null);
            result = await tcs.Task;
        }
        return result;
    }
    public async Task CopyToClipBoard(string message)
    {
        if (message is not null)
        {
            if (IsOnUIThread)
            {
                await topLevel!.Clipboard!.SetTextAsync(message);
            }
            else
            {
                uiContext?.Post(async _ =>
                {
                    await topLevel!.Clipboard!.SetTextAsync(message);
                }, null);
            }
        }
        //await Application.Current.Clipboard.SetTextAsync(message);
    }
    public async Task<bool> IsTextInClipBoard()
    {
        return !string.IsNullOrEmpty(await topLevel!.Clipboard!.GetTextAsync());
        //return Application.Current.Clipboard.ContainsText();
    }
}
