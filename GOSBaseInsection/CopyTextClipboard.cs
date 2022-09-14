using BaseLibrary;

namespace GOSBaseInjection;

public class CopyTextClipboard : ICopyTextClipboard
{
    Func<char, object, Task<object?>>? InteractionInvoke;
    public void SetInteractionInvoke(Func<char, object, Task<object?>>? interactionInvoke)
    {
        InteractionInvoke = interactionInvoke;
        if (InteractionInvoke is null)
            throw new NullReferenceException();
    }
    public async Task<string?> CopyFromClipBoard()
    {
        if (InteractionInvoke is null)
            throw new NullReferenceException();
        return (string?)await InteractionInvoke('V', null);
    }
    public async Task CopyToClipBoard(string message)
    {
        if (InteractionInvoke is null)
            throw new NullReferenceException();
        await InteractionInvoke('C', message);
    }
}
