using Avalonia.Threading;
using BaseLibrary;

namespace GOSBaseInsection;

public class AvaloniaUIThreadServices : IUIThreadServices
{
    //https://chatgpt.com/c/691de93c-1bc8-8330-aedf-449256368739
    public bool IsOnUIThread => Dispatcher.UIThread.CheckAccess();

    // Versão assíncrona (recomendada)
    public async Task<T> RunOnUIThreadAsync<T>(Func<T> func)
    {
        if (IsOnUIThread)
        {
            return func();
        }
        else
        {
            return await Dispatcher.UIThread.InvokeAsync(func);
        }
    }

    // Versão síncrona (se você realmente precisar bloquear)
    public T RunOnUIThread<T>(Func<T> func)
    {
        if (IsOnUIThread)
        {
            return func();
        }
        else
        {
            var op = Dispatcher.UIThread.InvokeAsync(func);
            // Bloqueia, mas agora usando o mecanismo correto do dispatcher
            return op.GetAwaiter().GetResult();
        }
    }
    public void RunOnUIThread(Action action)
    {
        if (IsOnUIThread)
        {
            action();
        }
        else
        {
            Dispatcher.UIThread.Invoke(action);
        }
    }
    public async Task RunOnUIThreadAsync(Action action)
    {
        if (IsOnUIThread)
        {
            action();
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(action);
        }
    }
}
