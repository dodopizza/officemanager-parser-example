using System.Net;
using AngleSharp.Html.Parser;
using OfficeManagerParserExample.Models;

namespace OfficeManagerParserExample;

internal static class Program
{
    private static readonly Configuration Config = Helpers.CreateConfiguration();
    
    private static readonly CookieContainer? OfficeManagerCookieContainer;

    private static readonly HttpClient AuthHttpClient =
        Helpers.CreateHttpClient(Config.AuthBaseUrl, out _);

    private static readonly HttpClient OfficeManagerHttpClient =
        Helpers.CreateHttpClient(Config.OfficeManagerBaseUrl, out OfficeManagerCookieContainer);

    private static readonly IHtmlParser Parser = new HtmlParser();

    internal static async Task Main()
    {
        var connectAuthorizeFormHtml = await GoToOfficeManagerDomainAsync();
        var connectAuthorizeFormData = await ParseConnectAuthorizeFormDataAsync(connectAuthorizeFormHtml);
        var accountLoginFormHtml = await SendConnectAuthorizeFormDataAsync(connectAuthorizeFormData);
        var accountLoginEmptyFormData = await ParseAccountLoginFormDataAsync(accountLoginFormHtml);
        var accountLoginFilledFormData = GetFilledAccountLoginFormData(accountLoginEmptyFormData);
        var signInOidcFormHtml = await SendAccountLoginFormDataAsync(accountLoginFilledFormData);
        var signInOidcFormData = await ParseSignInOidcFormDataAsync(signInOidcFormHtml);
        var selectRoleFormHtml = await SendSignInOidcFormDataAsync(signInOidcFormData);
        var selectRoleFormData = await ParseSelectRoleFormAsync(selectRoleFormHtml);
        var selectDepartmentFormHtml = await SendSelectRoleFormDataAsync(selectRoleFormData, Config.RoleId);
        var selectDepartmentFormData = await ParseSelectDepartmentFormAsync(selectDepartmentFormHtml);
        var operationalStatisticsHtml = await SendSelectDepartmentFormDataAsync(selectDepartmentFormData, Config.DepartmentUuid);

        PrintCookies();

        var operationalStatistics = await ParseOperationalStatisticsPageAsync(operationalStatisticsHtml);
        PrintOperationalStatisticsPage(operationalStatistics);
    }

    private static async Task<string> GoToOfficeManagerDomainAsync()
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get
        };

        var response = await OfficeManagerHttpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        return responseContent;
    }

    private static async Task<ConnectAuthorizeFormData> ParseConnectAuthorizeFormDataAsync(
        string connectAuthorizeFormHtml)
    {
        var document = await Parser.ParseDocumentAsync(connectAuthorizeFormHtml);

        var clientId = document.GetInputValue("client_id");
        var redirectUri = document.GetInputValue("redirect_uri");
        var responseType = document.GetInputValue("response_type");
        var scope = document.GetInputValue("scope");
        var codeChallenge = document.GetInputValue("code_challenge");
        var codeChallengeMethod = document.GetInputValue("code_challenge_method");
        var responseMode = document.GetInputValue("response_mode");
        var nonce = document.GetInputValue("nonce");
        var state = document.GetInputValue("state");

        return new ConnectAuthorizeFormData
        {
            ClientId = clientId,
            RedirectUri = redirectUri,
            ResponseType = responseType,
            Scope = scope,
            CodeChallenge = codeChallenge,
            CodeChallengeMethod = codeChallengeMethod,
            ResponseMode = responseMode,
            Nonce = nonce,
            State = state
        };
    }

    private static async Task<string> SendConnectAuthorizeFormDataAsync(ConnectAuthorizeFormData formData)
    {
        var formValues = new Dictionary<string, string?>
        {
            ["client_id"] = formData.ClientId,
            ["redirect_uri"] = formData.RedirectUri,
            ["response_type"] = formData.ResponseType,
            ["scope"] = formData.Scope,
            ["code_challenge"] = formData.CodeChallenge,
            ["code_challenge_method"] = formData.CodeChallengeMethod,
            ["response_mode"] = formData.ResponseMode,
            ["nonce"] = formData.Nonce,
            ["state"] = formData.State
        };

        var response = await AuthHttpClient.PostAsFormUrlEncodedAsync("connect/authorize", formValues);
        var responseContent = await response.Content.ReadAsStringAsync();
        return responseContent;
    }

    private static async Task<LoginFormData> ParseAccountLoginFormDataAsync(string accountLoginFormHtml)
    {
        var document = await Parser.ParseDocumentAsync(accountLoginFormHtml);

        var returnUrl = document.GetInputValue("ReturnUrl");
        var requestVerificationToken = document.GetInputValue("__RequestVerificationToken");

        return new LoginFormData
        {
            ReturnUrl = returnUrl,
            RequestVerificationToken = requestVerificationToken
        };
    }

    private static LoginFormData GetFilledAccountLoginFormData(LoginFormData emptyFormData)
    {
        return new LoginFormData
        {
            ReturnUrl = emptyFormData.ReturnUrl,
            Login = Config.Login,
            Password = Config.Password,
            AuthMethod = "local",
            RequestVerificationToken = emptyFormData.RequestVerificationToken,
            RememberLogin = false
        };
    }

    private static async Task<string> SendAccountLoginFormDataAsync(LoginFormData formData)
    {
        var formValues = new Dictionary<string, string?>
        {
            ["ReturnUrl"] = formData.ReturnUrl,
            ["Login"] = formData.Login,
            ["Password"] = formData.Password,
            ["authMethod"] = formData.AuthMethod,
            ["__RequestVerificationToken"] = formData.RequestVerificationToken,
            ["RememberLogin"] = formData.RememberLogin.ToString()
        };

        var response = await AuthHttpClient.PostAsFormUrlEncodedAsync("/login", formValues);
        var responseContent = await response.Content.ReadAsStringAsync();
        return responseContent;
    }

    private static async Task<SignInOidcFormData> ParseSignInOidcFormDataAsync(string signInOidcFormHtml)
    {
        var document = await Parser.ParseDocumentAsync(signInOidcFormHtml);

        var code = document.GetInputValue("code");
        var scope = document.GetInputValue("scope");
        var state = document.GetInputValue("state");
        var sessionState = document.GetInputValue("session_state");

        return new SignInOidcFormData
        {
            Code = code,
            Scope = scope,
            State = state,
            SessionState = sessionState
        };
    }

    private static async Task<string> SendSignInOidcFormDataAsync(SignInOidcFormData formData)
    {
        var formValues = new Dictionary<string, string?>
        {
            ["code"] = formData.Code,
            ["scope"] = formData.Scope,
            ["state"] = formData.State,
            ["session_state"] = formData.SessionState
        };

        var response = await OfficeManagerHttpClient.PostAsFormUrlEncodedAsync("signin-oidc", formValues);
        var responseContent = await response.Content.ReadAsStringAsync();

        return responseContent;
    }

    private static async Task<SelectRoleFormData> ParseSelectRoleFormAsync(string selectRoleFormHtml)
    {
        var document = await Parser.ParseDocumentAsync(selectRoleFormHtml);

        var requestVerificationToken = document.GetInputValue("__RequestVerificationToken");

        return new SelectRoleFormData
        {
            RequestVerificationToken = requestVerificationToken
        };
    }

    private static async Task<string> SendSelectRoleFormDataAsync(SelectRoleFormData formData, int? selectedRoleId)
    {
        var formValues = new Dictionary<string, string?>
        {
            ["roleId"] = selectedRoleId?.ToString(),
            ["__RequestVerificationToken"] = formData.RequestVerificationToken
        };

        var response = await OfficeManagerHttpClient.PostAsFormUrlEncodedAsync(
            "Infrastructure/Authenticate/SelectRole",
            formValues);
        var responseContent = await response.Content.ReadAsStringAsync();

        return responseContent;
    }

    private static async Task<SelectDepartmentFormData> ParseSelectDepartmentFormAsync(string selectDepartmentFormHtml)
    {
        var document = await Parser.ParseDocumentAsync(selectDepartmentFormHtml);

        var requestVerificationToken = document.GetInputValue("__RequestVerificationToken");

        return new SelectDepartmentFormData
        {
            RequestVerificationToken = requestVerificationToken
        };
    }

    private static async Task<string> SendSelectDepartmentFormDataAsync(SelectDepartmentFormData formData,
        string? selectedDepartmentUuid)
    {
        var formValues = new Dictionary<string, string?>
        {
            ["uuid"] = selectedDepartmentUuid,
            ["__RequestVerificationToken"] = formData.RequestVerificationToken
        };

        var response = await OfficeManagerHttpClient.PostAsFormUrlEncodedAsync(
            "Infrastructure/Authenticate/SelectDepartment",
            formValues);
        var responseContent = await response.Content.ReadAsStringAsync();

        return responseContent;
    }

    private static async Task<OperationalStatisticsPage> ParseOperationalStatisticsPageAsync(
        string operationalStatisticsHtml)
    {
        var document = await Parser.ParseDocumentAsync(operationalStatisticsHtml);

        var userInfoElement = document.QuerySelector(".info_user");
        var userFullName = userInfoElement?.Children[0].TextContent;
        var userLogin = userInfoElement?.Children[1].TextContent;

        var availableNavigationLinks = document.QuerySelectorAll(".navigation_linkBorderBlock")
            .Select(el => el.TextContent)
            .ToArray();

        return new OperationalStatisticsPage
        {
            UserLogin = userLogin,
            UserFullName = userFullName,
            AvailableNavigationLinks = availableNavigationLinks
        };
    }

    private static void PrintCookies()
    {
        var cookies = OfficeManagerCookieContainer?.GetAllCookies()
            .Select(c => $"{c.Name}={c.Value}")
            .ToArray();

        Console.WriteLine("Cookies to access OfficeManager:\n");
        Console.WriteLine($"{string.Join(';', cookies ?? Array.Empty<string>())}\n");
    }

    private static void PrintOperationalStatisticsPage(OperationalStatisticsPage operationalStatistics)
    {
        Console.WriteLine("Data from OfficeManager:\n");
        Console.WriteLine($"User login: {operationalStatistics.UserLogin}");
        Console.WriteLine($"User full name: {operationalStatistics.UserFullName}");
        Console.WriteLine($"Available links:\n\t{string.Join("\n\t", operationalStatistics.AvailableNavigationLinks)}");
    }
}