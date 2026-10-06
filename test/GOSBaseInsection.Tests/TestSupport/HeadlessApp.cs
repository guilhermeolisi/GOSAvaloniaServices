using Avalonia;
using Avalonia.Headless;
using GOSBaseInsection.Tests.TestSupport;

[assembly: AvaloniaTestApplication(typeof(HeadlessApp))]

// Paralelismo desligado no assembly: o Avalonia.Headless isola cada [AvaloniaFact] com estado GLOBAL do processo
// (AvaloniaLocator.EnterScope e Dispatcher.ResetBeforeUnitTests), como explica Nimloth.Views.Tests/TestParallelization.cs.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace GOSBaseInsection.Tests.TestSupport;

/// <summary>Aplicacao headless minima, sem tema: cada teste poe nos estilos da aplicacao o que precisa.</summary>
public class HeadlessApp : Application
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<HeadlessApp>().UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
