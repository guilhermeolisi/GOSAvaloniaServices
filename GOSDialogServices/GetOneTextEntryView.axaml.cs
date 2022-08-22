using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GOSAvaloniaServices;

public partial class GetOneTextEntryView : UserControl 
{
    public GetOneTextEntryView()
    {
        InitializeComponent();
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
