using System.Security.Claims;
using GrpcAuth.Contracts.Models.Requests;
using GrpcAuth.Contracts.Models.Wrapper;

namespace GrpcAuth.Client.Managers;

public interface IAuthenticationManager
{
    Task<Result> Login(TokenRequest request);
    Task<Result> Logout();
    Task<string> RefreshToken();
    Task<string> TryRefreshToken();
    Task<string> TryForceRefreshToken();
    Task<ClaimsPrincipal> CurrentUser();
}