using LiveChartsCore.Defaults;
using System.Collections.ObjectModel;

namespace GOSChartServices;

public interface IChartServices
{
    ObservablePoint?[] VerticalLine(double x, double y);
    void VerticalLine(ObservableCollection<ObservablePoint?> DataPoints, double x, double y);
}