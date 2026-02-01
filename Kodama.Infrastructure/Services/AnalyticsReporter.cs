using System.Text;
using System.Text.Json;
using Kodama.Application.Interfaces;
using Kodama.Shared.DTOs;

namespace Kodama.Infrastructure.Services;

public class AnalyticsReporter : IAnalyticsReporter
{
    public void PrintToConsole(AnalyticsReport report)
    {
        Console.WriteLine(FormatConsoleReport(report));
    }

    public string ExportJson(AnalyticsReport report)
    {
        return JsonSerializer.Serialize(report, new JsonSerializerOptions()
        {
            // PropertyNameCaseInsensitive = false,
            WriteIndented = true,
        });
    }

    public string ExportCsv(AnalyticsReport report)
    {
        throw new NotImplementedException();
    }

    public string ExportMarkdown(AnalyticsReport report)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("# Kodama Simulation Report");
        sb.AppendLine();
        sb.AppendLine($"**Generated**: {DateTimeOffset.FromUnixTimeMilliseconds(report.GeneratedAtUnixMs):yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();
        sb.AppendLine("## Summary");
        sb.AppendLine();
        sb.AppendLine($"| Metric | Value |");
        sb.AppendLine($"|--------|-------|");
        sb.AppendLine($"| Duration | {report.ElapsedSeconds:F1} seconds |");
        sb.AppendLine($"| Tasks Completed | {report.TotalTasksCompleted:N0} |");
        sb.AppendLine($"| Throughput | {report.Throughput:F2} tasks/sec |");
        sb.AppendLine();
        
        if (report.Stations?.Count > 0)
        {
            sb.AppendLine("## Station Metrics");
            sb.AppendLine();
            sb.AppendLine("| Station | Utilization | Current Queue | Peak Queue | Status |");
            sb.AppendLine("|---------|-------------|---------------|------------|--------|");
            foreach (var station in report.Stations)
            {
                var status = station.IsBottleneck ? "⚠️ WARN" : "✓ OK";
                sb.AppendLine($"| #{station.StationId} | {station.Utilization:P0} | {station.CurrentQueue} | {station.PeakQueue} | {status} |");
            }
            sb.AppendLine();
        }

        if (report.Recommendations?.Count > 0)
        {
            sb.AppendLine("## Recommendations");
            sb.AppendLine();
            foreach (var rec in report.Recommendations)
            {
                sb.AppendLine($"- {rec}");
            }
        }

        return sb.ToString();
    }

    private string FormatConsoleReport(AnalyticsReport report)
    {
        var sb = new StringBuilder();

        sb.AppendLine();
        sb.AppendLine("╔══════════════════════════════════════════════════════════╗");
        sb.AppendLine("║            KODAMA SIMULATION REPORT                      ║");
        sb.AppendLine("╚══════════════════════════════════════════════════════════╝");
        sb.AppendLine();

        sb.AppendLine($"  Duration:         {report.ElapsedSeconds:F1} seconds");
        sb.AppendLine($"  Tasks Completed:  {report.TotalTasksCompleted:N0}");
        sb.AppendLine($"  Throughput:       {report.Throughput:F2} tasks/second");
        sb.AppendLine();

        if (report.Stations?.Count > 0)
        {
            sb.AppendLine("  ┌──────────┬─────────────┬───────────┬───────────┬────────┐");
            sb.AppendLine("  │ Station  │ Utilization │ Cur Queue │ Peak Queue│ Status │");
            sb.AppendLine("  ├──────────┼─────────────┼───────────┼───────────┼────────┤");

            foreach (var station in report.Stations)
            {
                var status = station.IsBottleneck ? "⚠ WARN" : "✓ OK  ";
                sb.AppendLine($"  │ #{station.StationId,-7} │ {station.Utilization,10:P0} │ {station.CurrentQueue,9} │ {station.PeakQueue,9} │ {status} │");
            }

            sb.AppendLine("  └──────────┴─────────────┴───────────┴───────────┴────────┘");
            sb.AppendLine();
        }

        if (report.Recommendations?.Count > 0)
        {
            sb.AppendLine("  RECOMMENDATIONS:");
            foreach (var rec in report.Recommendations)
            {
                sb.AppendLine($"  • {rec}");
            }
            sb.AppendLine();
        }

        sb.AppendLine($"  Generated at: {DateTimeOffset.FromUnixTimeMilliseconds(report.GeneratedAtUnixMs):yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        return sb.ToString();
    }
}