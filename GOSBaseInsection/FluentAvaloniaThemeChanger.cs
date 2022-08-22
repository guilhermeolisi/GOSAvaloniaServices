using Avalonia;
using Avalonia.Styling;
using Avalonia.Threading;
using BaseLibrary;
using FluentAvalonia.Styling;
using Splat;

namespace GOSBaseInsection;

public class FluentAvaloniaThemeChanger : IThemeChanger
{
    IThemeCollection _themeCollection;
    IThemeCollection _transparencyCollection;

    public FluentAvaloniaThemeChanger(IThemeCollection? themeCollection = null, IThemeCollection? transparencyCollection = null)
    {
        _themeCollection = themeCollection ?? Locator.Current.GetService<IThemeCollection>("theme")!;
        _transparencyCollection = transparencyCollection ?? Locator.Current.GetService<IThemeCollection>("transparency");
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
            await SetTransparency(lastIsTransparent == true, lastTheme);
        }
        catch (Exception)
        {
            Console.WriteLine($"some error has executed");
        }
    }
    bool? lastIsTransparent;
    public async Task SetTransparency(bool isTransparent, char? type)
    {
        IThemeTransparencyBase theme = null;
        if (type is null)
            type = lastTheme;
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            for (int i = 0; i < _transparencyCollection.Length; i++)
            {
                if (_transparencyCollection[i].theme is IThemeTransparencyBase t && t.Transparency == isTransparent && t.Type == type)
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

            }
        });
        lastIsTransparent = isTransparent;

    }
    private Styles? FindTheme(char type)
    {
        for (int i = 0; i < _themeCollection.Length; i++)
        {
            if (type == _themeCollection[i].type && _themeCollection[i].theme is Styles styles)
                return styles;
        }
        return null;
    }
}