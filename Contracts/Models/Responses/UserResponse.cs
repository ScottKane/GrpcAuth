using ProtoBuf;

namespace GrpcAuth.Contracts.Models.Responses;

[ProtoContract]
public class UserResponse
{
    [ProtoMember(1)] public string? Id { get; set; }
    [ProtoMember(2)] public string? UserName { get; set; }
    [ProtoMember(3)] public string? FirstName { get; set; }
    [ProtoMember(4)] public string? LastName { get; set; }
    [ProtoMember(5)] public string? Email { get; set; }
    [ProtoMember(6)] public bool IsActive { get; set; } = true;
    [ProtoMember(7)] public bool EmailConfirmed { get; set; }
    [ProtoMember(8)] public string? PhoneNumber { get; set; }
    [ProtoMember(9)] public string? ProfilePictureDataUrl { get; set; }
}