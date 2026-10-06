using System.Reflection;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using BaseLibrary;
using FluentAssertions;
using FluentAvalonia.Styling;
using GOSBaseInjection;

namespace GOSBaseInsection.Tests;

/// <summary>
/// Com o tema 'S' (segue o sistema), a troca de cores do sistema tem de aplicar o tema certo ANTES de ler o accent e
/// avisar quem assina <see cref="FluentAvaloniaThemeChanger.SubscribeSystemColorUpdate"/>. O defeito que isto fixa
/// (CS4014): o handler chamava <c>SetTheme('S')</c> sem esperar, o <c>SetTheme</c> agenda a troca do estilo com
/// prioridade baixa no dispatcher, e o aviso saia com o estilo do tema anterior ainda na aplicacao.
/// <para>O evento se dispara pelo <c>OnColorValuesChanged</c> do <see cref="DefaultPlatformSettings"/>, que e o
/// <see cref="IPlatformSettings"/> do Avalonia.Headless; o metodo e protegido, por isso a reflexao.</para>
/// </summary>
public class FluentAvaloniaThemeChangerTests
{
    private sealed class FakeTheme(char type) : Styles, IThemeBase
    {
        public char Type => type;
    }

    private sealed class FakeProvider(List<(char type, IThemeBase theme)> themes) : IThemeCollectionProvider
    {
        public IEnumerable<(char type, IThemeBase theme)>? GetAllThemes() => themes;
    }

    private static void RaiseColorValuesChanged(PlatformColorValues values)
    {
        var settings = Application.Current!.PlatformSettings;
        settings.Should().BeAssignableTo<DefaultPlatformSettings>();
        var raise = typeof(DefaultPlatformSettings).GetMethod("OnColorValuesChanged", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        raise.Should().NotBeNull();
        raise!.Invoke(settings, [values]);
    }

    [AvaloniaFact]
    public async Task ColorValuesChanged_ComTemaDoSistema_AplicaOTemaAntesDeAvisarAsCores()
    {
        var app = Application.Current!;
        var light = new FakeTheme('L');
        var dark = new FakeTheme('D');
        app.Styles.Add(new FluentAvaloniaTheme());
        app.Styles.Add(light);
        var changer = new FluentAvaloniaThemeChanger(new FakeProvider([('L', light), ('D', dark)]), new FakeProvider([]));

        // Sistema claro, tema 'S': o estilo da aplicacao e o claro.
        (await changer.SetTheme('S')).Should().BeFalse();
        app.Styles.OfType<FakeTheme>().Should().ContainSingle().Which.Should().BeSameAs(light);

        IThemeBase? styleWhenNotified = null;
        var notified = new TaskCompletionSource();
        changer.SubscribeSystemColorUpdate(() =>
        {
            styleWhenNotified = app.Styles.OfType<FakeTheme>().Single();
            notified.TrySetResult();
        });

        // O sistema passa a escuro e muda o accent: a aplicacao segue a variante, e o changer tem de trocar o estilo.
        RaiseColorValuesChanged(new PlatformColorValues
        {
            ThemeVariant = PlatformThemeVariant.Dark,
            AccentColor1 = Color.FromArgb(255, 10, 120, 200),
        });
        app.ActualThemeVariant.Should().Be(ThemeVariant.Dark);

        (await Task.WhenAny(notified.Task, Task.Delay(TimeSpan.FromSeconds(10)))).Should().BeSameAs(notified.Task, "a troca de accent tem de avisar quem assinou");
        styleWhenNotified.Should().BeSameAs(dark, "o aviso sai depois de o tema do sistema estar aplicado");
        changer.SystemAccentColor.Should().Equal(255, 10, 120, 200);
    }
}
