using LiveChartsCore.Defaults;

namespace GOSChartServices;

public interface IChartServices
{
    ObservablePoint[] VerticalLine(double x, double y);
}