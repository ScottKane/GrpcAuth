using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GrpcAuth.Contracts.Models.Requests;
using GrpcAuth.Contracts.Models.Responses;
using GrpcAuth.Contracts.Models.Wrapper;
using GrpcAuth.Contracts.Services;
using GrpcAuth.Server.Configuration;
using GrpcAuth.Server.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GrpcAuth.Server.Services;

public class TokenService : ITokenService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationConfiguration _config;
    private readonly UserManager<ApplicationUser> _userManager;

    public TokenService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        SignInManager<ApplicationUser> signInManager,
        IOptions<ApplicationConfiguration> config)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _config = config.Value;
    }

    public async Task<Result<TokenResponse>> LoginAsync(TokenRequest model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null) return await Result<TokenResponse>.FailAsync("User Not Found.");

        if (!user.IsActive)
            return await Result<TokenResponse>.FailAsync("User Not Active. Please contact the administrator.");

        if (!user.EmailConfirmed) return await Result<TokenResponse>.FailAsync("E-Mail not confirmed.");

        var passwordValid = await _userManager.CheckPasswordAsync(
            user,
            model.Password);
        if (!passwordValid) return await Result<TokenResponse>.FailAsync("Invalid Credentials.");

        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
        await _userManager.UpdateAsync(user);

        var token = await GenerateJwtAsync(user);
        var response = new TokenResponse
        {
            Token = token,
            RefreshToken = user.RefreshToken
        };
        
        return await Result<TokenResponse>.SuccessAsync(response);
    }

    public async Task<Result<TokenResponse>> GetRefreshTokenAsync(RefreshTokenRequest? model)
    {
        if (model is null) return await Result<TokenResponse>.FailAsync("Invalid Client Token.");

        var userPrincipal = GetPrincipalFromExpiredToken(model.Token);
        var userEmail = userPrincipal.FindFirstValue(ClaimTypes.Email);
        var user = await _userManager.FindByEmailAsync(userEmail);
        if (user == null) return await Result<TokenResponse>.FailAsync("User Not Found.");
        if (user.RefreshToken != model.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            return await Result<TokenResponse>.FailAsync("Invalid Client Token.");
        var token = GenerateEncryptedToken(
            GetSigningCredentials(),
            await GetClaimsAsync(user));
        user.RefreshToken = GenerateRefreshToken();
        await _userManager.UpdateAsync(user);

        var response = new TokenResponse
        {
            Token = token,
            RefreshToken = user.RefreshToken,
            RefreshTokenExpiryTime = user.RefreshTokenExpiryTime
        };
        return await Result<TokenResponse>.SuccessAsync(response);
    }

    private async Task<string> GenerateJwtAsync(ApplicationUser user)
    {
        var token = GenerateEncryptedToken(
            GetSigningCredentials(),
            await GetClaimsAsync(user));
        return token;
    }

    private async Task<IEnumerable<Claim>> GetClaimsAsync(ApplicationUser user)
    {
        var userClaims = await _userManager.GetClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var roleClaims = new List<Claim>();
        var permissionClaims = new List<Claim>();
        foreach (var role in roles)
        {
            roleClaims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role));
            var thisRole = await _roleManager.FindByNameAsync(role);
            var allPermissionsForThisRoles = await _roleManager.GetClaimsAsync(thisRole);
            permissionClaims.AddRange(allPermissionsForThisRoles);
        }

        var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.FirstName!),
                new(ClaimTypes.Surname, user.LastName!),
                new(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty)
            }.Union(userClaims)
             .Union(roleClaims)
             .Union(permissionClaims);

        return claims;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private string GenerateEncryptedToken(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
    {
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(2),
            signingCredentials: signingCredentials);
        var tokenHandler = new JwtSecurityTokenHandler();
        var encryptedToken = tokenHandler.WriteToken(token);
        return encryptedToken;
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string? token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.Secret!)),
            ValidateIssuer = false,
            ValidateAudience = false,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.Zero
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(
            token,
            tokenValidationParameters,
            out var securityToken);
        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(
                SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }

    private SigningCredentials GetSigningCredentials()
    {
        var secret = Encoding.UTF8.GetBytes(_config.Secret!);
        return new SigningCredentials(
            new SymmetricSecurityKey(secret),
            SecurityAlgorithms.HmacSha256);
    }
}