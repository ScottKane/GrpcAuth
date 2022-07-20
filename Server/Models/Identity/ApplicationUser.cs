using Microsoft.AspNetCore.Identity;

namespace GrpcAuth.Server.Models.Identity;

public class ApplicationUser : IdentityUser<string>
{
    public bool IsActive { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public DateTime CreatedOn { get; set; }
    [ProtectedPersonalData] public string? FirstName { get; set; }
    [ProtectedPersonalData] public string? LastName { get; set; }
}