using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public interface IThemeChanger
{
    byte[] SystemAccentColor { get; }
    byte[] SystemAccentColorLigth { get; }
    byte[] SystemAccentColorDark { get; }

    void SetAccentColor(byte[]? color);
    Task<bool> SetTheme(char theme);
    Task SetTransparency(bool isTransparent, bool isOnlyPanel, char? type);
    void SubscribeSystemColorUpdate(Action action);
}
