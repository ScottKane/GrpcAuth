using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace GrpcAuth.Client.Authentication;

public class ApplicationAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;

    public ApplicationAuthenticationStateProvider(ILocalStorageService localStorage) => _localStorage = localStorage;

    public ClaimsPrincipal? AuthenticationStateUser { get; set; }

    public void MarkUserAsAuthenticated(string userName)
    {
        var authenticatedUser = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[]
                {
                    new Claim(
                        ClaimTypes.Name,
                        userName)
                },
                "apiauth"));

        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));

        NotifyAuthenticationStateChanged(authState);
    }

    public void MarkUserAsLoggedOut()
    {
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        var state = Task.FromResult(new AuthenticationState(anonymousUser));

        NotifyAuthenticationStateChanged(state);
    }

    public async Task<ClaimsPrincipal> GetAuthenticationStateProviderUserAsync() => (await GetAuthenticationStateAsync()).User;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync() =>
        await Task.Run(() =>
        {
            var token = _localStorage.GetItem<string>("authToken");
            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        
            var state = new AuthenticationState(
                new ClaimsPrincipal(
                    new ClaimsIdentity(
                        GetClaimsFromJwt(token),
                        "jwt")));
            AuthenticationStateUser = state.User;
        
            return state;
        });

    private static IEnumerable<Claim> GetClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs is null) return claims;
        keyValuePairs.TryGetValue(ClaimTypes.Role, out var roles);

        if (roles is not null)
        {
            if (roles.ToString()!.Trim().StartsWith("["))
            {
                var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString()!);

                if (parsedRoles != null)
                    claims.AddRange(
                        parsedRoles.Select(
                            role => new Claim(
                                ClaimTypes.Role,
                                role)));
            }
            else
                claims.Add(
                    new Claim(
                        ClaimTypes.Role,
                        roles.ToString()!));

            keyValuePairs.Remove(ClaimTypes.Role);
        }

        keyValuePairs.TryGetValue("Permission", out var permissions);
        if (permissions is not null)
        {
            if (permissions.ToString()!.Trim().StartsWith("["))
            {
                var parsedPermissions = JsonSerializer.Deserialize<string[]>(permissions.ToString()!);
                if (parsedPermissions != null)
                    claims.AddRange(
                        parsedPermissions.Select(
                            permission => new Claim(
                                "Permission",
                                permission)));
            }
            else
                claims.Add(
                    new Claim(
                        "Permission",
                        permissions.ToString()!));

            keyValuePairs.Remove("Permission");
        }

        claims.AddRange(
            keyValuePairs.Select(
                kvp => new Claim(
                    kvp.Key,
                    kvp.Value.ToString()!)));

        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64) =>
        Convert.FromBase64String((base64.Length % 4) switch
        {
            2 => base64 + "==",
            3 => base64 + "=",
            _ => base64
        });
}