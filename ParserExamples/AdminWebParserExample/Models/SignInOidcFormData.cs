namespace AdminWebParserExample.Models;

public class SignInOidcFormData
{
    public string? Code { get; init; }
    public string? Scope { get; init; }
    public string? State { get; init; }
    public string? SessionState { get; init; }
}