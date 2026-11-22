using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using BaseLibrary;
using BaseLibrary.DependencyInjection;
using FluentAvalonia.Styling;

namespace GOSBaseInjection;

public class FluentAvaloniaThemeChanger : IThemeChanger, IDisposable
{
    /// <summary>
    /// Solta o evento de cores do sistema, que e da aplicacao (global): sem isto um changer descartado continuaria
    /// trocando os estilos a cada mudanca do sistema (E182; no app ha um so, de vida do processo).
    /// </summary>
    public void Dispose()
    {
        if (Application.Current is { } app)
            app.PlatformSettings.ColorValuesChanged -= PlatformSettings_ColorValuesChanged;
        GC.SuppressFinalize(this);
    }

    private FluentAvaloniaTheme _faTheme;
    List<(char type, IThemeBase theme)>? _themes;
    List<(char type, IThemeBase theme)>? _transparencies;

    public byte[] SystemAccentColor { get; private set; }
    public byte[] SystemAccentColorLight1 { get; private set; }
    public byte[] SystemAccentColorLight2 { get; private set; }
    public byte[] SystemAccentColorLight3 { get; private set; }
    public byte[] SystemAccentColorDark1 { get; private set; }
    public byte[] SystemAccentColorDark2 { get; private set; }
    public byte[] SystemAccentColorDark3 { get; private set; }
    public FluentAvaloniaThemeChanger(IThemeCollectionProvider? themeCollection = null, IThemeCollectionProvider? transparencyCollection = null)
    {
        var _themeProvider = themeCollection ?? Locator.ConstantContainer.Resolve<IThemeCollectionProvider>("theme") ?? throw new ArgumentNullException(nameof(themeCollection), "themeCollection cannot be null");
        var _transparencyProvider = transparencyCollection ?? Locator.ConstantContainer.Resolve<IThemeCollectionProvider>("transparency") ?? throw new ArgumentNullException(nameof(transparencyCollection), "transparencyCollection cannot be null");
        _themes = _themeProvider.GetAllThemes() as List<(char type, IThemeBase theme)>;
        _transparencies = _transparencyProvider.GetAllThemes() as List<(char type, IThemeBase theme)>;

        Application.Current.PlatformSettings.ColorValuesChanged += PlatformSettings_ColorValuesChanged;
    }

    private void PlatformSettings_ColorValuesChanged(object? sender, Avalonia.Platform.PlatformColorValues e)
    {
        // async: o accent e as cores do sistema sao lidos DEPOIS de o tema 'S' ser aplicado (antes o
        // SetTheme corria solto e terminava depois do GetSystemColors, com prioridade de dispatcher mais baixa).
        Dispatcher.UIThread.Post(async () =>
        {
            if (lastTheme is null || lastTheme == 'S')
                await SetTheme('S');

            // CORREÇÃO: no snap (Linux), o portal xdg-desktop responde tarde no startup e o
            // FluentAvaloniaTheme não regenera seus resources de accent quando a cor chega por este
            // evento — o resource "SystemAccentColor" permanecia defasado. Empurramos a cor do sistema
            // explicitamente via CustomAccentColor (mesmo mecanismo de SetAccentColor, que comprovadamente
            // regenera as variações Light/Dark) para forçar o recompute antes de lermos os resources.
            if (_useSystemAccent)
                ApplySystemAccent(e.AccentColor1);

            GetSystemColors();
        }, DispatcherPriority.Background);
    }

    char? lastTheme;
    bool isDark;
    public async Task<bool> SetTheme(char theme)
    {
        GetFATheme();
        try
        {
            isDark = theme == 'D';
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                //var thm = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();

                //Uri uri;
                switch (theme)
                {
                    case 'L':
                        //thm.RequestedTheme = "Light";
                        Application.Current.RequestedThemeVariant = ThemeVariant.Light;
                        _faTheme.PreferSystemTheme = false;
                        break;
                    case 'D':
                        //thm.RequestedTheme = "Dark";
                        Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
                        _faTheme.PreferSystemTheme = false;
                        break;
                    case 'H':
                        //thm.RequestedTheme = "HighContrast";
                        Application.Current.RequestedThemeVariant = FluentAvaloniaTheme.HighContrastTheme;
                        _faTheme.PreferSystemTheme = false;
                        break;
                    case 'S':
                    default:
                        //Application.Current.RequestedThemeVariant = null;
                        // O PreferSystemTheme do FluentAvalonia poe na aplicacao a variante do sistema (e a segue); por
                        // vir de 'D' ou 'L' nao fica presa (E182, SetTheme_ShouldFollowTheSystem_WhenSystemComesAfterDark).
                        _faTheme.PreferSystemTheme = true;
                        isDark = Application.Current.ActualThemeVariant == ThemeVariant.Dark;
                        //thm.RequestedTheme = "Light";
                        break;
                }

                //Muda as customizações do tema do Nimloth
                char themeTemp = isDark ? 'D' : 'L';
                for (int i = 0; i < Application.Current.Styles.Count; i++)
                {
                    if ((Application.Current.Styles[i] is IThemeBase t && t is not IThemeTransparencyBase) || (Application.Current.Styles[i].Children.Count > 0 && Application.Current.Styles[i].Children[0] is IThemeBase t2 && t2 is not IThemeTransparencyBase))
                    {
                        Application.Current.Styles[i] = FindTheme(themeTemp);
                        break;
                    }
                }


            }, (DispatcherPriority)1);
            lastTheme = theme; //isDark ? 'D' : 'L'; //lastTheme = theme == 'D' ? 'D' : 'L';
            await SetTransparency(lastIsTransparent == true, lastIsFullTransparent == true, lastTheme);
            return isDark;
        }
        catch (Exception)
        {
            Console.WriteLine($"some error has executed");
        }
        return false;
    }
    bool? lastIsTransparent;
    bool? lastIsFullTransparent;
    public async Task SetTransparency(bool isTransparent, bool isAllTransparent, char? type)
    {
        IThemeTransparencyBase? theme = null;
        if (type is null)
            type = lastTheme;

        if (type == 'H')
            type = 'L';
        else if (type == 'S')
            type = isDark ? 'D' : 'L';

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            theme = FindTransparency(isTransparent, isAllTransparent, type);

            if (theme is not null)
            {
                for (int i = 0; i < Application.Current.Styles.Count; i++)
                {
                    if (Application.Current.Styles[i] is IThemeTransparencyBase || (Application.Current.Styles[i].Children.Count > 0 && Application.Current.Styles[i].Children[0] is IThemeTransparencyBase))
                    {
                        Application.Current.Styles[i] = theme as Styles;
                        break;
                    }
                }

            }
            else
            {
                //TODO ver o que fazer neste caso
            }
        });
        lastIsTransparent = isTransparent;
        lastIsFullTransparent = isAllTransparent;
    }
    public void SetAccentColor(byte[]? color)
    {
        GetFATheme();
        if (color?.Length != 4)
            color = null;
        if (color is null)
        {
            // Voltar a seguir a cor do sistema. No snap não basta CustomAccentColor=null (o FA não
            // re-lê do portal), então espelhamos explicitamente a accent atual do sistema.
            _useSystemAccent = true;
            ApplySystemAccent(Application.Current.PlatformSettings.GetColorValues().AccentColor1);
        }
        else
        {
            _useSystemAccent = false;
            _faTheme.PreferUserAccentColor = false;
            _faTheme.CustomAccentColor = new Color(color[0], color[1], color[2], color[3]);
        }

        GetSystemColors();
    }
    Action systemColorUpdate;
    public void SubscribeSystemColorUpdate(Action action) => systemColorUpdate = action;
    private void GetSystemColors()
    {
        // SystemAccentColor não é o color do OS, mas do próprio FluentAvalonia, então o CustomAccentColor aparecerá também nestes resources
        GetFATheme();
        bool changed = false;

        if (_faTheme.TryGetResource("SystemAccentColor", null, out var curColor))
        {
            Color color = (Color)curColor;

#if DEBUG
            var trash = Application.Current.PlatformSettings.GetColorValues();
#endif

            if (SystemAccentColor is null || SystemAccentColor[0] != color.A || SystemAccentColor[1] != color.R || SystemAccentColor[2] != color.G || SystemAccentColor[3] != color.B)
            {
                changed = true;
                SystemAccentColor = [color.A, color.R, color.G, color.B];
            }
        }
        if (_faTheme.TryGetResource("SystemAccentColorLight1", null, out curColor))
        {
            Color color = (Color)curColor;
            if (SystemAccentColorLight1 is null || SystemAccentColorLight1[0] != color.A || SystemAccentColorLight1[1] != color.R || SystemAccentColorLight1[2] != color.G || SystemAccentColorLight1[3] != color.B)
            {
                changed = true;
                SystemAccentColorLight1 = [color.A, color.R, color.G, color.B];
            }
        }
        if (_faTheme.TryGetResource("SystemAccentColorLight2", null, out curColor))
        {
            Color color = (Color)curColor;
            if (SystemAccentColorLight2 is null || SystemAccentColorLight2[0] != color.A || SystemAccentColorLight2[1] != color.R || SystemAccentColorLight2[2] != color.G || SystemAccentColorLight2[3] != color.B)
            {
                changed = true;
                SystemAccentColorLight2 = [color.A, color.R, color.G, color.B];
            }
        }
        if (_faTheme.TryGetResource("SystemAccentColorLight3", null, out curColor))
        {
            Color color = (Color)curColor;
            if (SystemAccentColorLight3 is null || SystemAccentColorLight3[0] != color.A || SystemAccentColorLight3[1] != color.R || SystemAccentColorLight3[2] != color.G || SystemAccentColorLight3[3] != color.B)
            {
                changed = true;
                SystemAccentColorLight3 = [color.A, color.R, color.G, color.B];
            }
        }
        if (_faTheme.TryGetResource("SystemAccentColorDark1", null, out curColor))
        {
            Color color = (Color)curColor;
            if (SystemAccentColorDark1 is null || SystemAccentColorDark1[0] != color.A || SystemAccentColorDark1[1] != color.R || SystemAccentColorDark1[2] != color.G || SystemAccentColorDark1[3] != color.B)
            {
                changed = true;
                SystemAccentColorDark1 = [color.A, color.R, color.G, color.B];
            }
        }
        if (_faTheme.TryGetResource("SystemAccentColorDark2", null, out curColor))
        {
            Color color = (Color)curColor;
            if (SystemAccentColorDark2 is null || SystemAccentColorDark2[0] != color.A || SystemAccentColorDark2[1] != color.R || SystemAccentColorDark2[2] != color.G || SystemAccentColorDark2[3] != color.B)
            {
                changed = true;
                SystemAccentColorDark2 = [color.A, color.R, color.G, color.B];
            }
        }
        if (_faTheme.TryGetResource("SystemAccentColorDark3", null, out curColor))
        {
            Color color = (Color)curColor;
            if (SystemAccentColorDark3 is null || SystemAccentColorDark3[0] != color.A || SystemAccentColorDark3[1] != color.R || SystemAccentColorDark3[2] != color.G || SystemAccentColorDark3[3] != color.B)
            {
                changed = true;
                SystemAccentColorDark3 = [color.A, color.R, color.G, color.B];
            }
        }
        if (changed)
            systemColorUpdate?.Invoke();
    }
    private void GetFATheme()
    {
        if (_faTheme is null)
        {
            _faTheme = Application.Current.Styles.FirstOrDefault(x => x is FluentAvaloniaTheme) as FluentAvaloniaTheme;
            // Sincroniza a accent inicial com o sistema. No desktop a cor já está disponível aqui; no
            // snap virá o default e será corrigida adiante pelo ColorValuesChanged quando o portal responder.
            if (_useSystemAccent)
                ApplySystemAccent(Application.Current.PlatformSettings.GetColorValues().AccentColor1);
            GetSystemColors();
        }
    }

    // true = espelhar a accent color do sistema; false = usar a cor custom escolhida pelo usuário.
    bool _useSystemAccent = true;

    // Empurra a accent color do sistema para o FluentAvaloniaTheme via CustomAccentColor, forçando-o a
    // recomputar SystemAccentColor + as variações Light1-3/Dark1-3. Necessário porque o caminho nativo
    // "PreferUserAccentColor" do FA não regenera os resources de forma confiável no snap (Linux).
    private void ApplySystemAccent(Color systemAccent)
    {
        // _faTheme nulo: ainda não inicializado; GetFATheme fará a aplicação inicial. Evita recursão.
        if (_faTheme is null)
            return;
        _faTheme.PreferUserAccentColor = false;
        _faTheme.CustomAccentColor = systemAccent;
    }
    /// <summary>
    /// O dicionario de transparencia do tipo pedido. Sem o par exato (o consumidor pode ter menos dicionarios que as
    /// combinacoes de <paramref name="isTransparent"/> e <paramref name="isAllTransparent"/>, como o Nimloth desde a
    /// E182, com um interruptor so), cai no de mesma <c>Transparency</c> e mesmo tipo, e por fim no primeiro do mesmo
    /// tipo; sem nenhum do tipo, devolve null e a aplicacao fica com o que tem.
    /// </summary>
    private IThemeTransparencyBase? FindTransparency(bool isTransparent, bool isAllTransparent, char? type)
    {
        var sameType = _transparencies?
            .Select(x => x.theme)
            .OfType<IThemeTransparencyBase>()
            .Where(t => t.Type == type)
            .ToList() ?? [];
        return sameType.LastOrDefault(t => t.Transparency == isTransparent && (!isTransparent || t.isAllTransparent == isAllTransparent))
            ?? sameType.FirstOrDefault(t => t.Transparency == isTransparent)
            ?? sameType.FirstOrDefault();
    }

    private Styles? FindTheme(char type)
    {
        for (int i = 0; i < _themes?.Count; i++)
        {
            if (type == _themes[i].type && _themes[i].theme is Styles styles)
                return styles;
        }
        return null;
    }
}