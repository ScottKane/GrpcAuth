using GrpcAuth.Contracts.Services;

namespace GrpcAuth.Server.Services;

public class TestService : ITestService
{
    public async Task<string> Echo(string message) =>
        await Task.Run(() =>
        {
            message = $"[Server][{DateTime.UtcNow}]: {message}";
            Console.WriteLine(message);
            return message;
        }).ConfigureAwait(false);
}