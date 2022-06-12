using System.ServiceModel;

namespace GrpcAuth.Contracts.Services;

[ServiceContract]
public interface ITestService
{
    [OperationContract] Task<string> Echo(string message);
}