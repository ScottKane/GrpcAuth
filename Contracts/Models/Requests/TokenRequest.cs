using ProtoBuf;

namespace GrpcAuth.Contracts.Models.Requests;

[ProtoContract]
public class TokenRequest
{
    [ProtoMember(1)] public string? Email { get; set; }
    [ProtoMember(2)] public string? Password { get; set; }
}