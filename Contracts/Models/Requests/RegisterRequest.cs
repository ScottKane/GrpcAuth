using ProtoBuf;

namespace GrpcAuth.Contracts.Models.Requests;

[ProtoContract]
public class RegisterRequest
{
    [ProtoMember(1)] public string? FirstName { get; set; }
    [ProtoMember(2)] public string? LastName { get; set; }
    [ProtoMember(3)] public string? Email { get; set; }
    // [ProtoMember(4)] public string? UserName { get; set; }
    [ProtoMember(4)] public string? Password { get; set; }
    [ProtoMember(5)] public string? ConfirmPassword { get; set; }
    // [ProtoMember(7)] public string? PhoneNumber { get; set; }
    [ProtoMember(6)] public bool ActivateUser { get; set; }
    [ProtoMember(7)] public bool AutoConfirmEmail { get; set; }
}