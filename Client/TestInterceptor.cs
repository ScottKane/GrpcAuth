using Grpc.Core;
using Grpc.Core.Interceptors;

namespace GrpcAuth.Client;

public class TestInterceptor : Interceptor
{
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        Console.WriteLine("Intercepted");
        
        return continuation(request, context);
    }
}