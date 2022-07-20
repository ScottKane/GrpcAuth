using System.ServiceModel;
using GrpcAuth.Contracts.Models.Requests;
using GrpcAuth.Contracts.Models.Responses;
using GrpcAuth.Contracts.Models.Wrapper;

namespace GrpcAuth.Contracts.Services;

[ServiceContract]
public interface ITokenService
{
    [OperationContract] Task<Result<TokenResponse>> LoginAsync(TokenRequest model);
    [OperationContract] Task<Result<TokenResponse>> GetRefreshTokenAsync(RefreshTokenRequest? model);
}