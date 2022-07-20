using System.ServiceModel;
using GrpcAuth.Contracts.Models.Requests;
using GrpcAuth.Contracts.Models.Wrapper;

namespace GrpcAuth.Contracts.Services;

[ServiceContract]
public interface IUserService
{
    [OperationContract] Task<Result> RegisterAsync(RegisterRequest request);
}