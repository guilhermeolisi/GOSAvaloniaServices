using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using BaseLibrary;
using FluentAvalonia.UI.Controls;

namespace GOSAvaloniaServices;

public class GOSDialogServices : IDialogService
{
    Dispatcher UIDispatcher = Dispatcher.UIThread;
    static string testResulString = "@#$%¨&*()_+:?}{`^<|";

    /// <summary>
    /// for "All Files" = Extensions = { "*" }
    /// </summary>
    /// <param name="extensions"></param>
    /// <param name="nameExt"></param>
    /// <param name="allowMultiple"></param>
    /// <param name="initialFolder"></param>
    /// <returns>return a array string of path of selected files</returns>
    public async Task<string[]?> OpenFile(Tuple<List<string>, string>[] extensions, bool allowMultiple, string? initialFolder, string? initialFile = null)
    {
        var desktopLifetime = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

        if (desktopLifetime is null || desktopLifetime.MainWindow is null || extensions is null)
            return null;

        string[]? result = new string[] { testResulString[..] };

        if (UIDispatcher.CheckAccess())
        {
            await method();
        }
        else
        {
            UIDispatcher.Post(async () =>
            {
                await method();
            }, DispatcherPriority.Send);

            while (!UIDispatcher.CheckAccess() && result is not null && result.Length == 1 && result[0] == testResulString) { }
        }
        return result;

        async Task method()
        {
            var dlg = new OpenFileDialog()
            {
                Title = "Open File" + (allowMultiple ? "(s)" : ""),
                AllowMultiple = allowMultiple,
                InitialFileName = initialFile,
            };

            for (int i = 0; i < extensions.Length; i++)
            {
                (dlg.Filters ??= new()).Add(new FileDialogFilter() { Name = extensions[i].Item2, Extensions = extensions[i].Item1 });
            }

            if (!string.IsNullOrWhiteSpace(initialFolder))
                dlg.Directory = initialFolder;
            var temp = dlg.ShowAsync(desktopLifetime.MainWindow);
            await temp;
            result = temp.Result;
        }
    }
    public async Task<string?> SaveFile(Tuple<List<string>, string>[] extensions, bool allowMultiple, string? initialFolder = null, string? initialFile = null)
    {
        var desktopLifetime = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

        if (desktopLifetime is null || desktopLifetime.MainWindow is null || extensions is null)
            return null;

        string? result = testResulString[..];
        if (UIDispatcher.CheckAccess())
        {
            await method();
        }
        else
        {
            UIDispatcher.Post(async () =>
            {
                await method();
            }, DispatcherPriority.Send);

            while (!UIDispatcher.CheckAccess() && result == testResulString) { }
        }
        return result;

        async Task method()
        {
            var dlg = new SaveFileDialog()
            {
                Title = "Save File",
                InitialFileName = initialFile,
            };
            for (int i = 0; i < extensions.Length; i++)
            {
                (dlg.Filters ??= new()).Add(new FileDialogFilter() { Name = extensions[i].Item2, Extensions = extensions[i].Item1 });
            }

            if (!string.IsNullOrWhiteSpace(initialFolder))
                dlg.Directory = initialFolder;

            result = await dlg.ShowAsync(desktopLifetime.MainWindow);
        }
    }
    public async Task<string?> SelectFolder(string? initialFolder = null)
    {
        var desktopLifetime = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

        if (desktopLifetime is null || desktopLifetime.MainWindow is null)
            return null;

        string? result = testResulString[..];

        if (UIDispatcher.CheckAccess())
        {
            await method();
        }
        else
        {
            UIDispatcher.Post(async () =>
            {
                await method();
            }, DispatcherPriority.Send);

            while (!UIDispatcher.CheckAccess() && result == testResulString[..]) { }
        }
        return result;

        async Task method()
        {
            var dlg = new OpenFolderDialog()
            {
                Title = "Select Folder",
            };
            if (!string.IsNullOrWhiteSpace(initialFolder))
                dlg.Directory = initialFolder;

            result = await dlg.ShowAsync(desktopLifetime.MainWindow);
        }
    }
    public async Task<bool?> ConfirmDialog(string message, string[] buttons)
    {
        var desktopLifetime = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

        if (desktopLifetime is null || desktopLifetime.MainWindow is null || string.IsNullOrWhiteSpace(message) || buttons is null || buttons.Length == 0)
            return null;

        ContentDialogResult? result = null;

        if (UIDispatcher.CheckAccess())
        {
            await method();
        }
        else
        {
            UIDispatcher.Post(async () =>
            {
                await method();

            }, DispatcherPriority.Send);

            while (!UIDispatcher.CheckAccess() && result is null) { }
        }

        //if (isAsync)
        //{
        //    result = await dialog.ShowAsync();
        //}
        //else
        //{
        //    result = dialog.ShowAsync().Result;
        //}
        if (result == ContentDialogResult.None)
            return null;
        else if (result == ContentDialogResult.Secondary)
            return false;
        else if (result == ContentDialogResult.Primary)
            return true;
        return null;

        async Task method()
        {
            var dlg = new ContentDialog()
            {
                Title = "Confirm",
                PrimaryButtonText = buttons[0],
                CloseButtonText = buttons[buttons.Length - 1],
                Content = message
            };
            if (buttons.Length > 1)
            {
                dlg.CloseButtonText = buttons[buttons.Length - 1];
                if (buttons.Length == 3)
                {
                    dlg.SecondaryButtonText = buttons[1];
                }
            }
            result = await dlg.ShowAsync();
        }
    }
    public async Task<string?> GetOneEntryDialog(string message, string entry, string[] buttons)
    {
        var desktopLifetime = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

        if (desktopLifetime is null || desktopLifetime.MainWindow is null || string.IsNullOrWhiteSpace(message) || buttons is null || buttons.Length != 2)
            return null;

        var contentVM = new GetOneTextEntryViewModel(message, entry);
        var content = new GetOneTextEntryView() { DataContext = contentVM };
        string initialEntry = entry is null ? null : entry[..];

        ContentDialogResult? result = null;
        if (UIDispatcher.CheckAccess())
        {
            await method();
        }
        else
        {
            UIDispatcher.Post(async () =>
            {
                await method();
            }, DispatcherPriority.Send);

            while (!UIDispatcher.CheckAccess() && result is null) { }
        }

        if (result == ContentDialogResult.None)
            return null;

        return contentVM.TextEntry;

        async Task method()
        {
            ContentDialog dlg = new ContentDialog()
            {
                Title = "Text Entry",
                PrimaryButtonText = buttons[0],
                CloseButtonText = buttons[buttons.Length - 1],
                DefaultButton = ContentDialogButton.Primary,
            };
            contentVM.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName != "TextEntry")
                    return;
                dlg.IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(contentVM.TextEntry) && contentVM.TextEntry != initialEntry;
            };
            dlg.Content = content;
            dlg.IsPrimaryButtonEnabled = false;
            result = await dlg.ShowAsync();
        }
    }
}