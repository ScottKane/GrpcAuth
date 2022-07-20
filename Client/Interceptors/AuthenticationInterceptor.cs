using Grpc.Core;
using Grpc.Core.Interceptors;
using GrpcAuth.Client.Managers;

namespace GrpcAuth.Client.Interceptors;

public class AuthenticationInterceptor : Interceptor
{
    private readonly IAuthenticationManager _authenticationManager;

    public AuthenticationInterceptor(IAuthenticationManager authenticationManager) => _authenticationManager = authenticationManager;

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        Task.Run(async () =>
        {
            try
            {
                var token = await _authenticationManager.TryRefreshToken();
                if (!string.IsNullOrEmpty(token))
                {
                    Console.WriteLine(token);
                    // _snackBar.Add(
                    //     "Refreshed Token.",
                    //     Severity.Success);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // _snackBar.Add(
                //     "You are Logged Out.",
                //     Severity.Error);
                // await _authenticationManager.Logout();
                // _navigationManager.NavigateTo("/");
            }
        });

        return continuation(request, context);
    }
}