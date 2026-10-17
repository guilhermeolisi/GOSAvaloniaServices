using ReactiveUI;
using ReactiveUI.Reactive;
using ReactiveUI.SourceGenerators;


namespace GOSAvaloniaServices;

public partial class GetOneTextEntryViewModel : ReactiveObject
{
    [Reactive]
    public partial string? TextEntry { get; set; }
    [Reactive]
    public partial string? Message { get; set; }
    public string? Watermark { get; set; }
    /// <summary>
    /// Só para design
    /// </summary>
    public GetOneTextEntryViewModel()
    {
        Message = "Message here";
        TextEntry = string.Empty;
        Watermark = "watermark";
    }
    public GetOneTextEntryViewModel(string message, string? entry, string? watermark)
    {
        Message = message;
        TextEntry = entry;
        Watermark = watermark;
    }
}
