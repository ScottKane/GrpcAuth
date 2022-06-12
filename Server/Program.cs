using GrpcAuth.Contracts.Services;
using GrpcAuth.Server.Services;
using Microsoft.AspNetCore;
using ProtoBuf.Grpc.Server;

namespace GrpcAuth.Server;

internal static class Program
{
    private static async Task Main() =>
        await WebHost.CreateDefaultBuilder()
            .UseStaticWebAssets()
            .ConfigureServices(services =>
            {
                services.AddCors(o => o.AddPolicy("AllowAll", p => p
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()));
                
                services.AddRazorPages();
                services.AddCodeFirstGrpc();
                services.AddSingleton<ITestService, TestService>();
            })
            .Configure((_, app) =>
            {
                app.UseBlazorFrameworkFiles();
                app.UseStaticFiles();
                app.UseRouting();
                app.UseGrpcWeb();
                
                app.UseCors("AllowAll");

                app.UseEndpoints(
                    endpoints =>
                    {
                        endpoints.MapRazorPages();
                        endpoints.MapGrpcService<TestService>().EnableGrpcWeb();
                        endpoints.MapFallbackToFile("index.html");
                    });
            })
            .Build()
            .RunAsync();
}