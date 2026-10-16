using System.Runtime.InteropServices;

namespace GOSAvaloniaServices;

/// <summary>
/// Politica do seletor de arquivos do <see cref="GOSDialogServices"/>: de qual pasta ele abre, e o que fazer quando o
/// seletor nativo do sistema falha.
/// </summary>
/// <remarks>
/// Excecao de producao de 16/10/2026 (Nimloth 1.0.12 e 1.0.15, quatro relatos na mesma maquina com Windows 10):
/// <see cref="COMException"/> "GetItemAt failed" (<c>E_NOINTERFACE</c>) dentro do <c>Win32StorageProvider</c> do
/// Avalonia, ao abrir o Import da Library. O usuario nao conseguia escolher arquivo nenhum, e repetir falhava igual.
/// </remarks>
public sealed class FilePickerFallback
{
    private readonly Func<string, bool> directoryExists;
    private readonly Func<string?> userDocumentsFolder;

    public FilePickerFallback(Func<string, bool>? directoryExists = null, Func<string?>? userDocumentsFolder = null)
    {
        this.directoryExists = directoryExists ?? Directory.Exists;
        this.userDocumentsFolder = userDocumentsFolder ?? (() => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
    }

    /// <summary>
    /// A pasta pedida, se existe; senao a pasta Documentos do usuario, se existe; senao <c>null</c>, e o seletor abre
    /// onde o sistema quiser.
    /// </summary>
    public string? ResolveStartFolder(string? initialFolder)
    {
        if (!string.IsNullOrWhiteSpace(initialFolder) && directoryExists(initialFolder))
            return initialFolder;
        string? documents = userDocumentsFolder();
        if (!string.IsNullOrWhiteSpace(documents) && directoryExists(documents))
            return documents;
        return null;
    }

    /// <summary>
    /// Abre o seletor nativo; se ele lancar <see cref="COMException"/>, abre o seletor gerenciado do Avalonia. Outra
    /// excecao segue para o chamador, que a reporta.
    /// </summary>
    public async Task<string[]?> OpenAsync(Func<Task<string[]?>> nativePicker, Func<Task<string[]?>> managedPicker)
    {
        try
        {
            return await nativePicker();
        }
        catch (COMException)
        {
            return await managedPicker();
        }
    }
}
