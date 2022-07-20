using ProtoBuf;

namespace GrpcAuth.Contracts.Models.Responses;

[ProtoContract]
public class TokenResponse
{
    [ProtoMember(1)] public string? Token { get; set; }
    [ProtoMember(2)] public string? RefreshToken { get; set; }
    [ProtoMember(3)] public DateTime? RefreshTokenExpiryTime { get; set; }
}