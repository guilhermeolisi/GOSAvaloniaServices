using LiveChartsCore.Defaults;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace GOSChartServices;

public interface IChartServices
{
    bool IsEqual(SKColor color1, SKColor color2);
    bool IsEqual(byte[] color1, byte[] color2);
    bool IsEqual(byte[] color1, SKColor color2);
    ObservablePoint?[] VerticalLine(double x, double y);
    void VerticalLine(ObservableCollection<ObservablePoint?> DataPoints, double x, double y);
}