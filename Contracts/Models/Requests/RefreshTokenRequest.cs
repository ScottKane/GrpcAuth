using ProtoBuf;

namespace GrpcAuth.Contracts.Models.Requests;

[ProtoContract]
public class RefreshTokenRequest
{
    [ProtoMember(1)] public string? Token { get; init; }
    [ProtoMember(2)] public string? RefreshToken { get; init; }
}