using System.Text;
using GrpcAuth.Contracts.Models.Requests;
using GrpcAuth.Contracts.Models.Wrapper;
using GrpcAuth.Contracts.Services;
using GrpcAuth.Server.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace GrpcAuth.Server.Services;

public class UserService : IUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
     private readonly RoleManager<ApplicationRole> _roleManager;
     private readonly UserManager<ApplicationUser> _userManager;

     public UserService(
         UserManager<ApplicationUser> userManager,
         RoleManager<ApplicationRole> roleManager,
         IHttpContextAccessor httpContextAccessor)
     {
         _userManager = userManager;
         _roleManager = roleManager;
         _httpContextAccessor = httpContextAccessor;
     }

     public async Task<Result> RegisterAsync(RegisterRequest request)
     {
         var user = new ApplicationUser
         {
             Email = request.Email,
             FirstName = request.FirstName,
             LastName = request.LastName,
             UserName = request.Email,
             IsActive = request.ActivateUser,
             EmailConfirmed = request.AutoConfirmEmail
         };

         var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
         if (userWithSameEmail != null) return await Result.FailAsync($"Email {request.Email} is already registered.");
         var result = await _userManager.CreateAsync(
             user,
             request.Password);
         if (result.Succeeded)
         {
             await _userManager.AddToRoleAsync(user, "Basic");
             if (!request.AutoConfirmEmail)
             {
                 var verificationUri = await GenerateConfirmationAddress(
                     user,
                     _httpContextAccessor.HttpContext!.Request.Headers["origin"]);
                 // Send activation email
             }

             return await Result<string>.SuccessAsync(
                 user.Id,
                 $"User {user.UserName} Registered.");
         }

         return await Result.FailAsync(
             result.Errors.Select(a => a.Description)
                 .ToList());

     }

     private async Task<string> GenerateConfirmationAddress(ApplicationUser user, string origin)
     {
         var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
         code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
         return $"{origin}/confirm/{user.Id}/{code}/";
     }
}