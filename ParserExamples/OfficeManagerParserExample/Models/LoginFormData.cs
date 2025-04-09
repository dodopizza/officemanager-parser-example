namespace OfficeManagerParserExample.Models;

public class LoginFormData
{
    public string? ReturnUrl { get; init; }
    public string? Login { get; init; }
    public string? Password { get; init; }
    public string? AuthMethod { get; init; }
    public bool RememberLogin { get; init; }
    public string? RequestVerificationToken { get; init; }
}