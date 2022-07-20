using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GrpcAuth.Client;
using GrpcAuth.Client.Authentication;
using GrpcAuth.Client.Interceptors;
using GrpcAuth.Client.Managers;
using GrpcAuth.Contracts.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using MudBlazor.Services;
using ProtoBuf.Grpc.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddMudServices();
builder.Services.AddLocalStorageServices();
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<ApplicationAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, ApplicationAuthenticationStateProvider>();
builder.Services.AddSingleton<AuthenticationInterceptor>();

builder.Services.AddSingleton(_ =>
{
    var handler = new GrpcWebHandler(GrpcWebMode.GrpcWebText, new HttpClientHandler());
    var credentials = CallCredentials.FromInterceptor(async (_, metadata) =>
    {
        var provider = builder.Services.BuildServiceProvider();
        var localStorage = provider.GetRequiredService<ILocalStorageService>();
        var authState = provider.GetRequiredService<AuthenticationStateProvider>();
        var token = localStorage.GetItem<string>("authToken");
        if (!string.IsNullOrEmpty(token))
        {
            metadata.Add("Authorization", $"Bearer {token}");
            var userIdentity = (await authState.GetAuthenticationStateAsync()).User.Identity;
            if (userIdentity!.IsAuthenticated)
                metadata.Add("User", userIdentity.Name!);
        }
    });
    var options = new GrpcChannelOptions
    {
        HttpHandler = handler,
        Credentials = ChannelCredentials.Create(new SslCredentials(), credentials)
    };

    return GrpcChannel.ForAddress("https://localhost:5001", options);
});

var provider = builder.Services.BuildServiceProvider();
var channel = provider.GetRequiredService<GrpcChannel>();

// NOT INTERCEPTED
builder.Services.AddSingleton(channel.CreateGrpcService<ITokenService>());
builder.Services.AddSingleton(channel.CreateGrpcService<IUserService>());

builder.Services.AddTransient<IAuthenticationManager, AuthenticationManager>();
provider = builder.Services.BuildServiceProvider();
var interceptor = provider.GetRequiredService<AuthenticationInterceptor>();
var invoker = channel.Intercept(interceptor);

// INTERCEPTED
// builder.Services.AddSingleton(invoker.CreateGrpcService<ICustomerService>());

await builder.Build().RunAsync();
await channel.ShutdownAsync();