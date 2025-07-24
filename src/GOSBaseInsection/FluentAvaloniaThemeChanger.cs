using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using BaseLibrary;
using FluentAvalonia.Styling;
using Splat;

namespace GOSBaseInjection;

public class FluentAvaloniaThemeChanger : IThemeChanger
{
    private FluentAvaloniaTheme _faTheme;
    List<(char type, IThemeBase theme)>? _themes;
    List<(char type, IThemeBase theme)>? _transparencies;

    public byte[] SystemAccentColor { get; private set; }
    public byte[] SystemAccentColorLight2 { get; private set; }
    public byte[] SystemAccentColorDark2 { get; private set; }
    public byte[] SystemAccentColorLight3 { get; private set; }
    public byte[] SystemAccentColorDark3 { get; private set; }
    public FluentAvaloniaThemeChanger(IThemeCollectionProvider? themeCollection = null, IThemeCollectionProvider? transparencyCollection = null)
    {
        var _themeProvider = themeCollection ?? Locator.Current.GetService<IThemeCollectionProvider>("theme") ?? throw new ArgumentNullException(nameof(themeCollection), "themeCollection cannot be null");
        var _transparencyProvider = transparencyCollection ?? Locator.Current.GetService<IThemeCollectionProvider>("transparency") ?? throw new ArgumentNullException(nameof(transparencyCollection), "transparencyCollection cannot be null");
        _themes = _themeProvider.GetAllThemes() as List<(char type, IThemeBase theme)>;
        _transparencies = _transparencyProvider.GetAllThemes() as List<(char type, IThemeBase theme)>;

        Application.Current.PlatformSettings.ColorValuesChanged += PlatformSettings_ColorValuesChanged;

    }

    private void PlatformSettings_ColorValuesChanged(object? sender, Avalonia.Platform.PlatformColorValues e)
    {
        if (lastTheme is null || lastTheme == 'S')
            SetTheme('S');
        GetSystemColors(); //não está funcionando para atualizar as cores do background da janela pq acho que ele é processado antes do FluentAvaloniaTheme
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
            for (int i = 0; i < _transparencies?.Count; i++)
            {
                if (_transparencies[i].theme is IThemeTransparencyBase t && t.Transparency == isTransparent && (!isTransparent || t.isAllTransparent == isAllTransparent) && t.Type == type)
                {
                    theme = t;
                }
            }

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
            _faTheme.PreferUserAccentColor = true;
            _faTheme.CustomAccentColor = null;
        }
        else
        {
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
            _faTheme.PreferUserAccentColor = true;
            GetSystemColors();
        }
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