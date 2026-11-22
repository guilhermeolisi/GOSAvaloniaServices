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

    // ---- E182: troca de tema idempotente e transparencia sem par exato ----

    private sealed class FakeTransparency(char type, bool transparency, bool allTransparent) : Styles, IThemeTransparencyBase
    {
        public char Type => type;
        public bool Transparency => transparency;
        public bool isAllTransparent => allTransparent;
    }

    /// <summary>Estilos da aplicacao do zero: o FluentAvaloniaTheme, um tema claro e um dicionario de transparencia.</summary>
    private static (Application app, FakeTheme light, FakeTheme dark) ResetStyles(Styles initialTransparency)
    {
        var app = Application.Current!;
        app.Styles.Clear();
        app.RequestedThemeVariant = ThemeVariant.Default;
        var light = new FakeTheme('L');
        var dark = new FakeTheme('D');
        app.Styles.Add(new FluentAvaloniaTheme());
        app.Styles.Add(light);
        app.Styles.Add(initialTransparency);
        return (app, light, dark);
    }

    [AvaloniaFact]
    public async Task SetTheme_ShouldGiveTheSameStylesAsACleanLight_WhenGoingLightDarkSystemLightWithTransparencyToggled()
    {
        // Arrange
        var solidLight = new FakeTransparency('L', false, false);
        var solidDark = new FakeTransparency('D', false, false);
        var glassLight = new FakeTransparency('L', true, false);
        var glassDark = new FakeTransparency('D', true, false);
        var (app, light, dark) = ResetStyles(solidLight);
        using var changer = new FluentAvaloniaThemeChanger(new FakeProvider([('L', light), ('D', dark)]),
            new FakeProvider([('L', solidLight), ('D', solidDark), ('L', glassLight), ('D', glassDark)]));
        await changer.SetTheme('L');
        var clean = app.Styles.ToList();

        // Act
        await changer.SetTheme('L');
        await changer.SetTransparency(true, false, null);
        await changer.SetTheme('D');
        await changer.SetTransparency(false, false, null);
        await changer.SetTheme('S');
        await changer.SetTransparency(true, false, null);
        await changer.SetTheme('L');
        await changer.SetTransparency(false, false, null);

        // Assert
        app.Styles.Should().Equal(clean, "a mesma escolha final da a mesma composicao de estilos, qualquer que seja o caminho");
        app.Styles.OfType<FakeTheme>().Should().ContainSingle().Which.Should().BeSameAs(light);
        app.Styles.OfType<FakeTransparency>().Should().ContainSingle().Which.Should().BeSameAs(solidLight);
        app.RequestedThemeVariant.Should().Be(ThemeVariant.Light);
    }

    [AvaloniaFact]
    public async Task SetTheme_ShouldFollowTheSystem_WhenSystemComesAfterDark()
    {
        // Arrange: o sistema do Avalonia.Headless e claro.
        var solidLight = new FakeTransparency('L', false, false);
        var solidDark = new FakeTransparency('D', false, false);
        var (app, light, dark) = ResetStyles(solidLight);
        using var changer = new FluentAvaloniaThemeChanger(new FakeProvider([('L', light), ('D', dark)]),
            new FakeProvider([('L', solidLight), ('D', solidDark)]));
        (await changer.SetTheme('D')).Should().BeTrue();
        app.ActualThemeVariant.Should().Be(ThemeVariant.Dark);

        // Act
        bool isDark = await changer.SetTheme('S');

        // Assert
        isDark.Should().BeFalse("o sistema e claro, e 'S' segue o sistema, nao o 'D' anterior");
        app.ActualThemeVariant.Should().Be(ThemeVariant.Light);
        app.Styles.OfType<FakeTheme>().Should().ContainSingle().Which.Should().BeSameAs(light);
        app.Styles.OfType<FakeTransparency>().Should().ContainSingle().Which.Should().BeSameAs(solidLight);
    }

    [AvaloniaFact]
    public async Task SetTransparency_ShouldUseTheSameTypeDictionary_WhenThereIsNoExactPair()
    {
        // Arrange: so ha um dicionario translucido por tipo, sem a variante "all transparent".
        var solidDark = new FakeTransparency('D', false, false);
        var glassDark = new FakeTransparency('D', true, false);
        var glassLight = new FakeTransparency('L', true, false);
        var (app, light, dark) = ResetStyles(solidDark);
        using var changer = new FluentAvaloniaThemeChanger(new FakeProvider([('L', light), ('D', dark)]),
            new FakeProvider([('D', solidDark), ('D', glassDark), ('L', glassLight)]));

        // Act
        Func<Task> act = () => changer.SetTransparency(true, true, 'D');

        // Assert
        await act.Should().NotThrowAsync();
        app.Styles.OfType<FakeTransparency>().Should().ContainSingle().Which.Should().BeSameAs(glassDark,
            "sem o par exato, vale o translucido do mesmo tipo");

        // E sem nenhum da mesma Transparency, o primeiro do tipo: nao fica o dicionario do tipo errado.
        await changer.SetTransparency(false, false, 'L');
        app.Styles.OfType<FakeTransparency>().Should().ContainSingle().Which.Should().BeSameAs(glassLight);
    }
}
