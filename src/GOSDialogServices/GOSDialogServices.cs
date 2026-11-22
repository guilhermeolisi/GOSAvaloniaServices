using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Dialogs;
using Avalonia.Dialogs.Internal;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;

namespace GOSAvaloniaServices;

public class GOSDialogServices : IDialogService
{
    Dispatcher UIDispatcher = Dispatcher.UIThread;
    static string testResulString = "@#$%¨&*()_+:?}{`^<|";
    private readonly FilePickerFallback filePicker = new();

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
            Window window = desktopLifetime.MainWindow;
            result = await filePicker.OpenAsync(native, managed);

            async Task<string[]?> native()
            {
                IStorageProvider storage = window.StorageProvider;
                FilePickerOpenOptions options = await BuildOpenOptionsAsync(storage, filePicker, extensions, allowMultiple, initialFolder, initialFile);
                IReadOnlyList<IStorageFile> files = await storage.OpenFilePickerAsync(options);
                return files.Select(f => f.TryGetLocalPath() ?? f.Name).ToArray();
            }

            // Seletor gerenciado do Avalonia so nesta chamada: o ManagedStorageProvider e interno, e o
            // UseManagedSystemDialogs trocaria o seletor do app inteiro. O OpenFileDialog.ShowManagedAsync saiu na
            // Avalonia 12 (E153): a janela do ManagedFileChooser e montada aqui, como o ManagedStorageProvider a monta.
            async Task<string[]?> managed()
            {
                FilePickerOpenOptions options = await BuildOpenOptionsAsync(window.StorageProvider, filePicker, extensions, allowMultiple, initialFolder, initialFile);
                var model = new ManagedFileChooserViewModel(options, new ManagedFileDialogOptions());
                var chooser = new Window()
                {
                    Title = options.Title,
                    Content = new ManagedFileChooser(),
                    DataContext = model,
                };
                string[]? chosen = null;
                model.CompleteRequested += items =>
                {
                    chosen = items;
                    chooser.Close();
                };
                model.CancelRequested += chooser.Close;
                await chooser.ShowDialog(window);
                return chosen;
            }
        }
    }

    internal static async Task<FilePickerOpenOptions> BuildOpenOptionsAsync(IStorageProvider storage, FilePickerFallback picker,
        Tuple<List<string>, string>[] extensions, bool allowMultiple, string? initialFolder, string? initialFile)
        => new FilePickerOpenOptions()
        {
            Title = "Open File" + (allowMultiple ? "(s)" : ""),
            AllowMultiple = allowMultiple,
            SuggestedFileName = initialFile,
            FileTypeFilter = ToFileTypes(extensions),
            SuggestedStartLocation = await picker.ResolveStartLocationAsync(storage, initialFolder),
        };

    internal static async Task<FilePickerSaveOptions> BuildSaveOptionsAsync(IStorageProvider storage, FilePickerFallback picker,
        Tuple<List<string>, string>[] extensions, string? initialFolder, string? initialFile)
        => new FilePickerSaveOptions()
        {
            Title = "Save File",
            SuggestedFileName = initialFile,
            FileTypeChoices = ToFileTypes(extensions),
            SuggestedStartLocation = await StartLocationAsync(storage, picker, initialFolder),
        };

    /// <summary>
    /// Save e SelectFolder: sem pasta pedida, nenhuma sugestao (o sistema abre na ultima pasta usada, como antes da
    /// E153); com pasta pedida que nao existe, Documentos. O Open ja caia em Documentos com pasta nula.
    /// </summary>
    private static async Task<IStorageFolder?> StartLocationAsync(IStorageProvider storage, FilePickerFallback picker, string? initialFolder)
        => string.IsNullOrWhiteSpace(initialFolder) ? null : await picker.ResolveStartLocationAsync(storage, initialFolder);

    internal static async Task<FolderPickerOpenOptions> BuildFolderOptionsAsync(IStorageProvider storage, FilePickerFallback picker, string? initialFolder)
        => new FolderPickerOpenOptions()
        {
            Title = "Select Folder",
            AllowMultiple = false,
            SuggestedStartLocation = await StartLocationAsync(storage, picker, initialFolder),
        };
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
            IStorageProvider storage = desktopLifetime.MainWindow.StorageProvider;
            FilePickerSaveOptions options = await BuildSaveOptionsAsync(storage, filePicker, extensions, initialFolder, initialFile);
            IStorageFile? file = await storage.SaveFilePickerAsync(options);
            result = file is null ? null : file.TryGetLocalPath() ?? file.Name;
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
            IStorageProvider storage = desktopLifetime.MainWindow.StorageProvider;
            FolderPickerOpenOptions options = await BuildFolderOptionsAsync(storage, filePicker, initialFolder);
            IReadOnlyList<IStorageFolder> folders = await storage.OpenFolderPickerAsync(options);
            result = folders.Count > 0 ? folders[0].TryGetLocalPath() ?? folders[0].Name : null;
        }
    }

    // Cada extensao vira o padrao "*.ext" (com ou sem o ponto ou o asterisco de quem chama), e "*", o "todos os
    // arquivos", fica "*": o "*.*" deixava de fora, no Linux, o arquivo sem extensao (E153).
    internal static List<FilePickerFileType> ToFileTypes(Tuple<List<string>, string>[] extensions)
        => extensions.Select(e => new FilePickerFileType(e.Item2) { Patterns = e.Item1.Select(ToPattern).ToList() }).ToList();

    private static string ToPattern(string extension)
        => extension == "*" ? "*" : "*." + extension.TrimStart('*', '.');
    public async Task<bool?> ConfirmDialog(string message, string[] buttons)
    {
        var desktopLifetime = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

        if (desktopLifetime is null || desktopLifetime.MainWindow is null || string.IsNullOrWhiteSpace(message) || buttons is null || buttons.Length == 0)
            return null;

        FAContentDialogResult? result = null;

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
        if (result == FAContentDialogResult.None)
            return null;
        else if (result == FAContentDialogResult.Secondary)
            return false;
        else if (result == FAContentDialogResult.Primary)
            return true;
        return null;

        async Task method()
        {
            ConfirmDialogButtons layout = ConfirmButtons(buttons);
            var dlg = new FAContentDialog()
            {
                Title = "Confirm",
                PrimaryButtonText = layout.Primary,
                CloseButtonText = layout.Close,
                DefaultButton = layout.Default,
                Content = message
            };
            if (layout.Secondary is not null)
                dlg.SecondaryButtonText = layout.Secondary;
            dlg.Classes.Add(MessageDialogClass);
            result = await dlg.ShowAsync();
        }
    }

    /// <summary>
    /// Classe dos dialogos de mensagem (confirmacao e entrada de texto): o app que consome o servico limita a largura deles
    /// por um estilo nessa classe (E179). O <see cref="FromViewModelDialog"/> nao a leva: mede pelo conteudo.
    /// </summary>
    public const string MessageDialogClass = "gos-message";

    /// <summary>Os textos dos botoes do <see cref="ConfirmDialog"/> e o botao do Enter.</summary>
    internal sealed record ConfirmDialogButtons(string Primary, string? Secondary, string Close, FAContentDialogButton Default);

    /// <summary>
    /// O primeiro botao e o Primary (devolve true), o do meio, com tres, o Secondary (false), e o ultimo o Close (null).
    /// O Enter fica no ULTIMO, o seguro: todo chamador poe o verbo primeiro e o "Cancel" por ultimo (E179, Parte 5).
    /// </summary>
    internal static ConfirmDialogButtons ConfirmButtons(string[] buttons)
        => new(buttons[0], buttons.Length == 3 ? buttons[1] : null, buttons[^1], FAContentDialogButton.Close);
    public async Task<string?> GetOneEntryDialog(string message, string entry, string waterMark, bool canSameInitialEntry, string[] buttons)
    {
        var desktopLifetime = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;


        if (message is null)
            throw new ArgumentNullException(nameof(message));
        if (message is null)
            throw new ArgumentException("the message can't be empty", nameof(message));

        if (buttons is null)
            throw new ArgumentNullException(nameof(buttons));
        if (buttons.Length != 2)
            throw new ArgumentOutOfRangeException(nameof(buttons), "must be a array with lenght 2");

        if (desktopLifetime is null || desktopLifetime.MainWindow is null)
            throw new ApplicationException("The app must have a window to show the dialog");

        var contentVM = new GetOneTextEntryViewModel(message, entry, waterMark);
        var content = new GetOneTextEntryView() { DataContext = contentVM };
        string initialEntry = entry is null ? null : entry[..];

        FAContentDialogResult? result = null;
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

        if (result is null || result == FAContentDialogResult.None)
            return null;

        return contentVM.TextEntry;

        async Task method()
        {
            FAContentDialog dlg = new FAContentDialog()
            {
                Title = "Text Entry",
                PrimaryButtonText = buttons[0],
                CloseButtonText = buttons[buttons.Length - 1],
                DefaultButton = FAContentDialogButton.Primary,
            };
            contentVM.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName != "TextEntry")
                    return;
                dlg.IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(contentVM.TextEntry) && (canSameInitialEntry || contentVM.TextEntry != initialEntry);
            };
            dlg.Content = content;
            dlg.Classes.Add(MessageDialogClass);
            //dlg.IsPrimaryButtonEnabled = false;
            result = await dlg.ShowAsync();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="viewmodel">The viewmodel must have a valid view to the viewlocator</param>
    /// <param name="title"></param>
    /// <param name="buttons"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="ApplicationException"></exception>
    public async Task<bool?> FromViewModelDialog(object viewmodel, string? title, string[] buttons, byte[]? background)
    {
        var desktopLifetime = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

        if (viewmodel is null)
            throw new ArgumentNullException(nameof(viewmodel));
        if (buttons is null)
            throw new ArgumentNullException(nameof(buttons));
        if (buttons.Length == 0 || buttons.Length > 3)
            throw new ArgumentOutOfRangeException(nameof(buttons), "must be a array with length between 1 or 3");
        if (background is not null && background.Length != 4)
            throw new ArgumentOutOfRangeException(nameof(background), "must be a array with length between 4. Alpha and RGB");
        if (desktopLifetime is null || desktopLifetime.MainWindow is null)
            throw new ApplicationException("The app must have a window to show the dialog");

        FAContentDialogResult? result = null;
        if (UIDispatcher.CheckAccess())
        {
            await method();
        }
        else
        {
            var waitHandle = new ManualResetEvent(false);
            UIDispatcher.Post(async () =>
            {
                await method();
                waitHandle.Set();
            }, DispatcherPriority.Send);
            waitHandle.WaitOne();
            //while (!UIDispatcher.CheckAccess() && result is null) { }
        }

        if (result is null || result == FAContentDialogResult.None)
            return null;

        return result == FAContentDialogResult.Primary;

        async Task method()
        {
            FAContentDialog dlg = new()
            {
                Title = title,
                PrimaryButtonText = buttons[0],
                DefaultButton = FAContentDialogButton.Primary,
            };
            if (buttons.Length >= 2)
            {
                dlg.CloseButtonText = buttons[buttons.Length - 1];
            }
            if (buttons.Length == 3)
            {
                dlg.CloseButtonText = buttons[1];
                dlg.IsSecondaryButtonEnabled = true;
            }
            if (background is not null)
                dlg.Background = new SolidColorBrush(new Color(background[0], background[1], background[2], background[3]));
            dlg.Content = viewmodel;
            result = await dlg.ShowAsync();
        }
    }
}