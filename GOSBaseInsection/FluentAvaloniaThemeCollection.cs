using BaseLibrary;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOSBaseInsection;

public class FluentAvaloniaThemeCollection : IThemeCollection
{
    public FluentAvaloniaThemeCollection(List<(char type, IThemeBase theme)>? themes = null)
    {
        Themes = themes ?? Locator.Current.GetService<List<(char, IThemeBase)>>()!;
    }

    public List<(char type, IThemeBase theme)> Themes { get; private set; }

}
