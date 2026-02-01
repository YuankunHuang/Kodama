using Kodama.Shared.DTOs;

namespace Kodama.Application.Interfaces;

public interface IAnalyticsReporter
{
    void PrintToConsole(AnalyticsReport report);

    string ExportJson(AnalyticsReport report);
    
    string ExportCsv(AnalyticsReport report);
    
    string ExportMarkdown(AnalyticsReport report);
}