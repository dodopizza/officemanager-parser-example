namespace OfficeManagerParserExample;

public class Configuration
{
    public string? AuthBaseUrl { get; init; }
    public string? OfficeManagerBaseUrl { get; init; }

    public string? Username { get; init; }
    public string? Password { get; init; }
    
    public int? RoleId { get; init; }
    public string? DepartmentUuid { get; init; }
}