namespace OfficeManagerParserExample.Models;

public class AccountLoginFormData
{
    public string? ReturnUrl { get; init; }
    public string? Login { get; init; }
    public string? Password { get; init; }
    public string? TenantName { get; init; }
    public string? CountryCode { get; init; }
    public string? AuthMethod { get; init; }
    public bool RememberLogin { get; init; }
    public string? RequestVerificationToken { get; init; }
}