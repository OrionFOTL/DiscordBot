using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SkiaSharp;

namespace DiscordBot.Features.DailyStats.Charting;

public class DailyStatsChartProvider : IDailyStatsChartProvider
{
    public Stream GetDailyActivityChart(IReadOnlyList<KeyValuePair<string, int>> ranking)
    {
        var barChart = new SKCartesianChart()
        {
            Width = 800,
            Height = 500,
            Title = new LabelVisual()
            {
                Text = "Users with most messages in the last 24 hours",
                VerticalAlignment = Align.Start,
                Padding = new() { Top = 10 },
                TextSize = 20,
                Paint = new SolidColorPaint(SKColors.Black),
            },
            Series =
            [
                new ColumnSeries<int>()
                {
                    Values = ranking.Select(x => x.Value).Take(1).ToList(),
                    Fill = new SolidColorPaint(SKColors.Gold),
                    DataLabelsPaint = new SolidColorPaint(SKColors.Gray),
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.End,
                    MaxBarWidth = double.MaxValue,
                    Padding = 20,
                    IgnoresBarPosition = true,
                },
                new ColumnSeries<int>()
                {
                    Values = [0, .. ranking.Select(x => x.Value).Skip(1).Take(1)],
                    Fill = new SolidColorPaint(SKColors.Silver),
                    DataLabelsPaint = new SolidColorPaint(SKColors.Gray),
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.End,
                    MaxBarWidth = double.MaxValue,
                    Padding = 20,
                    IgnoresBarPosition = true,
                },
                new ColumnSeries<int>()
                {
                    Values = [0, 0, .. ranking.Select(x => x.Value).Skip(2).Take(1)],
                    Fill = new SolidColorPaint(SKColors.Brown),
                    DataLabelsPaint = new SolidColorPaint(SKColors.Gray),
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.End,
                    MaxBarWidth = double.MaxValue,
                    Padding = 20,
                    IgnoresBarPosition = true,
                },
                new ColumnSeries<int>()
                {
                    Values = [0, 0, 0, .. ranking.Select(x => x.Value).Skip(3)],
                    DataLabelsPaint = new SolidColorPaint(SKColors.Gray),
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.End,
                    MaxBarWidth = double.MaxValue,
                    Padding = 20,
                    IgnoresBarPosition = true,
                },
            ],
            XAxes =
            [
                new Axis
                {
                    Labels = ranking.Select(x => Ellipsize(x.Key, 20)).ToList(),
                    LabelsRotation = -20,
                    Padding = new() { Top = 5 },
                    TicksPaint = new SolidColorPaint(new SKColor(35, 35, 35)),
                    TicksAtCenter = true,
                    ForceStepToMin = true,
                    MinStep = 1,
                }
            ],
            YAxes =
            [
                new Axis
                {
                    MinLimit = 0,
                    SubseparatorsCount = 9,
                    SubseparatorsPaint = new SolidColorPaint(SKColors.LightGray) { PathEffect = new DashEffect([7, 3]) },
                    SeparatorsPaint = new SolidColorPaint(SKColors.DarkGray),
                }
            ]
        };

        return barChart.GetImage().Encode().AsStream();
    }

    private static string Ellipsize(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
        {
            return text;
        }

        var ellipsis = "...";
        return string.Concat(text.AsSpan(0, maxLength - ellipsis.Length), ellipsis);
    }
}
