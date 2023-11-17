using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SkiaSharp;

namespace DiscordBot.Features.DailyStats.Charting;

public interface IDailyStatsChartProvider
{
    Stream GetDailyActivityChart(IReadOnlyList<TopAuthor> ranking);
}

internal class DailyStatsChartProvider : IDailyStatsChartProvider
{
    private static SolidColorPaint BlackOpenSansPaint => new(SKColors.Black) { SKTypeface = SKTypeface.FromFamilyName("Open Sans") };

    public Stream GetDailyActivityChart(IReadOnlyList<TopAuthor> ranking)
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
                Paint = BlackOpenSansPaint,
            },
            Series = GetColumnSeries(ranking.Select(ta => ta.MessageCount)).ToList(),
            XAxes =
            [
                new Axis
                {
                    Labels = ranking.Select(ta => Ellipsize(ta.Author.ToString(), 20)).ToList(),
                    LabelsRotation = -20,
                    LabelsPaint = BlackOpenSansPaint,
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
                    LabelsPaint = BlackOpenSansPaint,
                    MinLimit = 0,
                    SubseparatorsCount = 9,
                    SubseparatorsPaint = new SolidColorPaint(SKColors.LightGray) { PathEffect = new DashEffect([7, 3]) },
                    SeparatorsPaint = new SolidColorPaint(SKColors.DarkGray),
                }
            ]
        };

        return barChart.GetImage().Encode().AsStream();
    }

    private IEnumerable<ColumnSeries<TValue?>> GetColumnSeries<TValue>(IEnumerable<TValue?> values)
    {
        var valuesEnumerator = values.GetEnumerator();
        var fillEnumerator = GetColumnFill().GetEnumerator();

        int i = 0;
        while (fillEnumerator.MoveNext())
        {
            if (valuesEnumerator.MoveNext())
            {
                yield return MakeColumnSeries(
                    [.. Enumerable.Repeat(default(TValue), i), valuesEnumerator.Current],
                    new SolidColorPaint(fillEnumerator.Current));
            }
            else
            {
                yield break;
            }

            i++;
        }

        var restOfValues = Enumerable.Repeat(default(TValue), i).ToList();
        while (valuesEnumerator.MoveNext())
        {
            restOfValues.Add(valuesEnumerator.Current);
        }

        yield return MakeColumnSeries(restOfValues, new SolidColorPaint(SKColors.CornflowerBlue));
    }

    private static IEnumerable<SKColor> GetColumnFill()
    {
        yield return SKColors.Gold;
        yield return SKColors.Silver;
        yield return SKColors.Brown;
    }

    private static ColumnSeries<TValue?> MakeColumnSeries<TValue>(IEnumerable<TValue?> values, SolidColorPaint? fill)
    {
        return new ColumnSeries<TValue?>()
        {
            Values = values,
            Fill = fill,
            DataLabelsPaint = BlackOpenSansPaint,
            DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.End,
            MaxBarWidth = double.MaxValue,
            Padding = 10,
            IgnoresBarPosition = true,
        };
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
