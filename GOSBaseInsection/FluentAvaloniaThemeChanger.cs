using Avalonia;
using Avalonia.Styling;
using Avalonia.Threading;
using BaseLibrary;
using FluentAvalonia.Styling;
using Splat;

namespace GOSBaseInjection;

public class FluentAvaloniaThemeChanger : IThemeChanger
{
    //IThemeCollectionProvider _themeProvider;
    //IThemeCollectionProvider _transparencyProvider;
    List<(char type, IThemeBase theme)>? _themes;
    List<(char type, IThemeBase theme)>? _transparencies;
    public FluentAvaloniaThemeChanger(IThemeCollectionProvider? themeCollection = null, IThemeCollectionProvider? transparencyCollection = null)
    {
        var _themeProvider = themeCollection ?? Locator.Current.GetService<IThemeCollectionProvider>("theme")!;
        var _transparencyProvider = transparencyCollection ?? Locator.Current.GetService<IThemeCollectionProvider>("transparency")!;
        _themes = _themeProvider.GetAllThemes() as List<(char type, IThemeBase theme)>;
        _transparencies = _transparencyProvider.GetAllThemes() as List<(char type, IThemeBase theme)>;
    }
    char? lastTheme;
    public async Task SetTheme(char theme)
    {
        try
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var thm = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();
                //Uri uri;
                switch (theme)
                {
                    case 'L':
                        thm.RequestedTheme = "Light";
                        break;
                    case 'D':
                        thm.RequestedTheme = "Dark";
                        break;
                    case 'H':
                        thm.RequestedTheme = "HighContrast";
                        break;
                    default:
                        thm.RequestedTheme = "Light";
                        break;
                }
                for (int i = 0; i < Application.Current.Styles.Count; i++)
                {
                    if ((Application.Current.Styles[i] is IThemeBase t && t is not IThemeTransparencyBase) || (Application.Current.Styles[i].Children.Count > 0 && Application.Current.Styles[i].Children[0] is IThemeBase t2 && t2 is not IThemeTransparencyBase))
                    {
                        Application.Current.Styles[i] = FindTheme(theme);
                        break;
                    }
                }


            }, (DispatcherPriority)1);
            lastTheme = theme == 'D' ? 'D' : 'L';
            await SetTransparency(lastIsTransparent == true, lastIsFullTransparent == true, lastTheme);
        }
        catch (Exception)
        {
            Console.WriteLine($"some error has executed");
        }
    }
    bool? lastIsTransparent;
    bool? lastIsFullTransparent;
    public async Task SetTransparency(bool isTransparent, bool isAllTransparent, char? type)
    {
        IThemeTransparencyBase theme = null;
        if (type is null)
            type = lastTheme;
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