namespace BaseLibrary;

public interface IThemeChanger
{
    byte[] SystemAccentColor { get; }
    byte[] SystemAccentColorLight1 { get; }
    byte[] SystemAccentColorLight2 { get; }
    byte[] SystemAccentColorLight3 { get; }
    byte[] SystemAccentColorDark1 { get; }
    byte[] SystemAccentColorDark2 { get; }
    byte[] SystemAccentColorDark3 { get; }
    void SetAccentColor(byte[]? color);
    Task<bool> SetTheme(char theme);
    Task SetTransparency(bool isTransparent, bool isAllTransparent, char? type);
    void SubscribeSystemColorUpdate(Action action);
}
