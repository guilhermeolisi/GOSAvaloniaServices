using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOSAvaloniaServices;

public class GetOneTextEntryViewModel : ReactiveObject
{
    [Reactive]
    public string? TextEntry { get; set; }
    [Reactive]
    public string? Message { get; set; }
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
