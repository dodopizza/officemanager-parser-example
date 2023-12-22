﻿using System.Net;
using AngleSharp.Html.Parser;
using ShiftManagerParserExample.Models;

namespace ShiftManagerParserExample;

internal static class Program
{
    private static readonly Configuration Config = Helpers.CreateConfiguration();

    private static readonly CookieContainer? ShiftManagerCookieContainer;
    private static readonly CookieContainer? AuthCookieContainer;

    private static readonly HttpClient AuthHttpClient =
        Helpers.CreateHttpClient(Config.AuthBaseUrl, out AuthCookieContainer);

    private static readonly HttpClient ShiftManagerHttpClient =
        Helpers.CreateHttpClient(Config.ShiftManagerBaseUrl, out ShiftManagerCookieContainer);

    private static readonly IHtmlParser Parser = new HtmlParser();

    internal static async Task Main()
    {
        var connectAuthorizeFormHtml = await GoToShiftManagerDomainAsync();
        var connectAuthorizeFormData = await ParseConnectAuthorizeFormDataAsync(connectAuthorizeFormHtml);
        var accountLoginFormHtml = await SendConnectAuthorizeFormDataAsync(connectAuthorizeFormData);
        var accountLoginEmptyFormData = await ParseAccountLoginFormDataAsync(accountLoginFormHtml);
        var accountLoginFilledFormData = GetFilledAccountLoginFormData(accountLoginEmptyFormData);
        var signInOidcFormHtml = await SendAccountLoginFormDataAsync(accountLoginFilledFormData);
        var signInOidcFormData = await ParseSignInOidcFormDataAsync(signInOidcFormHtml);
        await SendSignInOidcFormDataAsync(signInOidcFormData);
        await SendSelectRoleFormDataAsync();

        PrintCookies();
    }

    private static async Task<string> GoToShiftManagerDomainAsync()
    {
        var requestUri = new Uri(ShiftManagerHttpClient.BaseAddress!, "/Infrastructure/Authenticate/Oidc");

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = requestUri
        };

        var response = await ShiftManagerHttpClient.SendAsync(request);
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

    private static async Task<AccountLoginFormData> ParseAccountLoginFormDataAsync(string accountLoginFormHtml)
    {
        var document = await Parser.ParseDocumentAsync(accountLoginFormHtml);

        var returnUrl = document.GetInputValue("ReturnUrl");
        var requestVerificationToken = document.GetInputValue("__RequestVerificationToken");

        return new AccountLoginFormData
        {
            ReturnUrl = returnUrl,
            RequestVerificationToken = requestVerificationToken
        };
    }

    private static AccountLoginFormData GetFilledAccountLoginFormData(AccountLoginFormData emptyFormData)
    {
        return new AccountLoginFormData
        {
            ReturnUrl = emptyFormData.ReturnUrl,
            Username = Config.Username,
            Password = Config.Password,
            TenantName = "dodopizza",
            CountryCode = "Ru",
            AuthMethod = "local",
            RequestVerificationToken = emptyFormData.RequestVerificationToken,
            RememberLogin = false
        };
    }

    private static async Task<string> SendAccountLoginFormDataAsync(AccountLoginFormData formData)
    {
        var formValues = new Dictionary<string, string?>
        {
            ["ReturnUrl"] = formData.ReturnUrl,
            ["Username"] = formData.Username,
            ["Password"] = formData.Password,
            ["TenantName"] = formData.TenantName,
            ["CountryCode"] = formData.CountryCode,
            ["authMethod"] = formData.AuthMethod,
            ["__RequestVerificationToken"] = formData.RequestVerificationToken,
            ["RememberLogin"] = formData.RememberLogin.ToString()
        };

        var response = await AuthHttpClient.PostAsFormUrlEncodedAsync("account/login", formValues);
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

    private static async Task SendSignInOidcFormDataAsync(SignInOidcFormData formData)
    {
        var formValues = new Dictionary<string, string?>
        {
            ["code"] = formData.Code,
            ["scope"] = formData.Scope,
            ["state"] = formData.State,
            ["session_state"] = formData.SessionState
        };

        var response = await ShiftManagerHttpClient.PostAsFormUrlEncodedAsync("signin-oidc", formValues);
    }

    private static async Task SendSelectRoleFormDataAsync()
    {
        var formValues = $"{{\"role\":\"ShiftManager\",\"departmentId\":\"{Config.DepartmentUuid}\"}}";

        await ShiftManagerHttpClient.PostAsJsonAsync(
            "Infrastructure/Authenticate/SetRole",
            formValues);
    }

    private static void PrintCookies()
    {
        var shiftManagerCookies = ShiftManagerCookieContainer?.GetAllCookies()
            .Select(c => $"{c.Name}={c.Value}")
            .ToArray();

        Console.WriteLine("Cookies to access ShiftManager:\n");
        Console.WriteLine($"{string.Join(Environment.NewLine, shiftManagerCookies ?? Array.Empty<string>())}\n");
    }
}