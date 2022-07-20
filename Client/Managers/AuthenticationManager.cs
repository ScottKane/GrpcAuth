using System.Security.Claims;
using GrpcAuth.Client.Authentication;
using GrpcAuth.Contracts.Models.Requests;
using GrpcAuth.Contracts.Models.Wrapper;
using GrpcAuth.Contracts.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace GrpcAuth.Client.Managers;

public class AuthenticationManager : IAuthenticationManager
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ITokenService _tokenService;
    private readonly ILocalStorageService _localStorage;

    public AuthenticationManager(
        ILocalStorageService localStorage,
        AuthenticationStateProvider authenticationStateProvider,
        ITokenService tokenService)
    {
        _localStorage = localStorage;
        _authenticationStateProvider = authenticationStateProvider;
        _tokenService = tokenService;
    }

    public async Task<ClaimsPrincipal> CurrentUser() => (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;

    public async Task<Result> Login(TokenRequest request)
    {
        var result = await _tokenService.LoginAsync(request);
        if (!result.Succeeded) return await Result.FailAsync(result.Messages);
        
        var token = result.Data!.Token;
        var refreshToken = result.Data.RefreshToken;
        _localStorage.SetItem("authToken", token);
        _localStorage.SetItem("refreshToken", refreshToken);

        ((ApplicationAuthenticationStateProvider) _authenticationStateProvider).MarkUserAsAuthenticated(request.Email!);
            
        return await Result.SuccessAsync();
    }

    public async Task<Result> Logout()
    {
        _localStorage.RemoveItem("authToken");
        _localStorage.RemoveItem("refreshToken");
        ((ApplicationAuthenticationStateProvider) _authenticationStateProvider).MarkUserAsLoggedOut();
        
        return await Result.SuccessAsync();
    }

    public async Task<string> RefreshToken()
    {
        var token = _localStorage.GetItem<string>("authToken");
        var refreshToken = _localStorage.GetItem<string>("refreshToken");
        var result = await _tokenService.GetRefreshTokenAsync(new RefreshTokenRequest {Token = token, RefreshToken = refreshToken});

        if (!result.Succeeded)
            throw new ApplicationException("Something went wrong during the refresh token action");

        token = result.Data!.Token;
        refreshToken = result.Data.RefreshToken;
        _localStorage.SetItem("authToken", token);
        _localStorage.SetItem("refreshToken", refreshToken);
        
        return token!;
    }

    public async Task<string> TryRefreshToken()
    {
        var availableToken = _localStorage.GetItem<string>("refreshToken");
        if (string.IsNullOrEmpty(availableToken)) return string.Empty;
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var exp = user.FindFirst(c => c.Type.Equals("exp"))?.Value;
        var expTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(exp));
        var timeUtc = DateTime.UtcNow;
        var diff = expTime - timeUtc;
        if (diff.TotalMinutes <= 1) return await RefreshToken();
        
        return string.Empty;
    }

    public async Task<string> TryForceRefreshToken() => await RefreshToken();
}