using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ScottPlot;

namespace digioz.Portal.Web.Areas.Admin.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogService _logService;
        private readonly IVisitorInfoService _visitorInfoService;

        public IndexModel(ILogService logService, IVisitorInfoService visitorInfoService)
        {
            _logService = logService;
            _visitorInfoService = visitorInfoService;
        }

        public IReadOnlyList<digioz.Portal.Bo.Log> LatestLogs { get; private set; } = Array.Empty<digioz.Portal.Bo.Log>();
        public IReadOnlyList<digioz.Portal.Bo.VisitorInfo> LatestVisitorLogs { get; private set; } = Array.Empty<digioz.Portal.Bo.VisitorInfo>();
        public string VisitorChartBase64 { get; private set; } = string.Empty;

        public void OnGet()
        {
            // Fetch last 5 logs using service method
            LatestLogs = _logService.GetLastN(5, "desc").ToList();

            // Fetch last 5 visitor logs using service method
            LatestVisitorLogs = _visitorInfoService.GetLastN(5, "desc").ToList();

            // Generate visitor tracking chart
            VisitorChartBase64 = GenerateVisitorTrackingChart();
        }

        private string GenerateVisitorTrackingChart()
        {
            try
            {
                // Calculate date 15 days ago
                var endDate = DateTime.UtcNow.Date;
                var startDate = endDate.AddDays(-14); // Last 15 days including today

                // Get unique visitor counts by date from service (efficient query)
                var visitorCounts = _visitorInfoService.GetUniqueVisitorCountsByDate(startDate, endDate);

                // Create arrays for all 15 days (fill missing days with 0)
                var dates = new List<DateTime>();
                var counts = new List<double>();

                for (int i = 0; i < 15; i++)
                {
                    var currentDate = startDate.AddDays(i);
                    dates.Add(currentDate);

                    // Get count from dictionary or default to 0 if date not present
                    visitorCounts.TryGetValue(currentDate, out var count);
                    counts.Add(count);
                }

                // Create the plot
                using (var plot = new Plot())
                {
                    // Add bar plot
                    var bars = plot.Add.Bars(counts.ToArray());

                    // Set bar color and labels
                    foreach (var bar in bars.Bars)
                    {
                        bar.FillColor = ScottPlot.Color.FromHex("#0d6efd"); // Bootstrap primary blue
                        bar.Label = bar.Value.ToString("F0"); // Show value as label on each bar
                    }

                    // Customize value label style
                    bars.ValueLabelStyle.Bold = true;
                    bars.ValueLabelStyle.FontSize = 12;
                    bars.ValueLabelStyle.ForeColor = ScottPlot.Color.FromHex("#212529");

                    // Configure X-axis (Bottom) - dates under each bar
                    plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
                         dates.Select((d, i) => new Tick((double)i, d.ToString("MM/dd"))).ToArray()
                     );
                    plot.Axes.Bottom.MajorTickStyle.Length = 5;
                    plot.Axes.Bottom.MajorTickStyle.Width = 1;
                    plot.Axes.Bottom.MajorTickStyle.Color = ScottPlot.Color.FromHex("#212529");

                    // X-axis styling - dark solid line
                    plot.Axes.Bottom.FrameLineStyle.Color = ScottPlot.Color.FromHex("#212529");
                    plot.Axes.Bottom.FrameLineStyle.Width = 2;

                    // X-axis label - positioned below dates
                    plot.Axes.Bottom.Label.Text = "Day";
                    plot.Axes.Bottom.Label.Bold = true;
                    plot.Axes.Bottom.Label.FontSize = 14;
                    plot.Axes.Bottom.Label.ForeColor = ScottPlot.Color.FromHex("#212529");

                    // X-axis tick labels styling
                    plot.Axes.Bottom.TickLabelStyle.ForeColor = ScottPlot.Color.FromHex("#212529");
                    plot.Axes.Bottom.TickLabelStyle.FontSize = 11;

                    // Configure Y-axis (Left)
                    plot.Axes.Left.MajorTickStyle.Length = 5;
                    plot.Axes.Left.MajorTickStyle.Width = 1;
                    plot.Axes.Left.MajorTickStyle.Color = ScottPlot.Color.FromHex("#212529");

                    // Y-axis styling - dark solid line
                    plot.Axes.Left.FrameLineStyle.Color = ScottPlot.Color.FromHex("#212529");
                    plot.Axes.Left.FrameLineStyle.Width = 2;

                    // Y-axis label - rotated sideways (to the left)
                    plot.Axes.Left.Label.Text = "Unique Visitors Count";
                    plot.Axes.Left.Label.Bold = true;
                    plot.Axes.Left.Label.FontSize = 14;
                    plot.Axes.Left.Label.ForeColor = ScottPlot.Color.FromHex("#212529");
                    plot.Axes.Left.Label.Rotation = -90;

                    // Y-axis tick labels styling
                    plot.Axes.Left.TickLabelStyle.ForeColor = ScottPlot.Color.FromHex("#212529");
                    plot.Axes.Left.TickLabelStyle.FontSize = 11;

                    // Add title
                    plot.Title("Visitor Tracking - Last 15 Days");

                    // Add margins for labels
                    plot.Axes.Margins(bottom: 0, top: 0.2);

                    // Generate image and convert to base64
                    var imageBytes = plot.GetImage(800, 400).GetImageBytes();
                    return Convert.ToBase64String(imageBytes);
                }
            }
            catch (Exception)
            {
                // Return empty string if chart generation fails
                return string.Empty;
            }
        }
    }
}
