using LiveChartsCore.Defaults;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace GOSChartServices;

public class ChartServices : IChartServices
{
    public ObservablePoint?[] VerticalLine(double x, double y)
    {
        ObservablePoint?[] points = new ObservablePoint?[3];
        points[0] = new ObservablePoint(x, null);
        points[1] = new ObservablePoint(x, 0);
        points[2] = new ObservablePoint(x, y);
        return points;
    }
    public void VerticalLine(ObservableCollection<ObservablePoint?> DataPoints, double x, double y)
    {
        ObservablePoint?[] temp = VerticalLine(x, y);
        for (int i = 0; i < temp.Length; i++)
        {
            DataPoints.Add(temp[i]);
        }
    }
    public bool IsEqual(SKColor color1, SKColor color2)
    {
        return color1.Alpha == color2.Alpha &&
               color1.Red == color2.Red &&
               color1.Green == color2.Green &&
               color1.Blue == color2.Blue;
    }
    public bool IsEqual(byte[] color1, byte[] color2)
    {
        if (color1 is null || color2 is null || color1.Length != 4 || color2.Length != 4)
            return false;
        return color1[0] == color2[0] &&
               color1[1] == color2[1] &&
               color1[2] == color2[2] &&
               color1[3] == color2[3];
    }
    public bool IsEqual(byte[] color1, SKColor color2)
    {
        if (color1 is null || color1.Length != 4)
            return false;
        return color1[0] == color2.Alpha &&
               color1[1] == color2.Red &&
               color1[2] == color2.Green &&
               color1[3] == color2.Blue;
    }
}
