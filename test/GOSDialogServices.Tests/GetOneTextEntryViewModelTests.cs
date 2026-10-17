using System.ComponentModel;

using FluentAssertions;

using GOSAvaloniaServices;

namespace GOSDialogServices.Tests;

/// <summary>
/// <see cref="GetOneTextEntryViewModel"/>: as propriedades <c>[Reactive]</c> avisam a mudanca. Canario da troca do
/// Fody pelo gerador de codigo (E154): sem o aviso, o dialogo de uma entrada de texto nao atualiza a tela.
/// </summary>
public class GetOneTextEntryViewModelTests
{
    [Theory]
    [InlineData(nameof(GetOneTextEntryViewModel.TextEntry))]
    [InlineData(nameof(GetOneTextEntryViewModel.Message))]
    public void ReactiveProperty_ShouldRaisePropertyChanged_WhenItChanges(string propriedade)
    {
        var viewModel = new GetOneTextEntryViewModel("message", "entry", "watermark");
        var avisos = new List<string?>();
        ((INotifyPropertyChanged)viewModel).PropertyChanged += (_, e) => avisos.Add(e.PropertyName);

        if (propriedade == nameof(GetOneTextEntryViewModel.TextEntry))
            viewModel.TextEntry = "outra entrada";
        else
            viewModel.Message = "outra mensagem";

        avisos.Should().Equal(propriedade);
    }
}
