using LiveChartsCore.Defaults;
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
}
