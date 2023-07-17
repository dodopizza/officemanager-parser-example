namespace OfficeManagerParserExample.Models;

public class OperationalStatisticsPage
{
    public string? UserLogin { get; init; }
    public string? UserFullName { get; init; }
    public string[] AvailableNavigationLinks { get; init; } = Array.Empty<string>();
}