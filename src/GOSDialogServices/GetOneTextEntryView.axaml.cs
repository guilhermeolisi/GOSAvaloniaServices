using Avalonia.Controls;
using Avalonia.Interactivity;

namespace GOSAvaloniaServices;

public partial class GetOneTextEntryView : UserControl
{
    public GetOneTextEntryView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// O campo recebe o foco quando o dialogo abre, com o texto selecionado para ser trocado direto: quem abre um pedido de
    /// texto vai digitar, e o Enter aciona o Primary (DefaultButton) (E183, A148; NIMLOTH-UX.md 6.6).
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (this.FindControl<TextBox>("entry") is { } box)
        {
            box.Focus();
            box.SelectAll();
        }
    }
}
