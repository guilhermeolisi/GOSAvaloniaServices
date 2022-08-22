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
    public GetOneTextEntryViewModel(string message, string? entry)
    {
        Message = message;
        TextEntry = entry;
    }
}
