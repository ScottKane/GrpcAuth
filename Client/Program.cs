using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using GrpcAuth.Contracts.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GrpcAuth.Client;
using MudBlazor.Services;
using ProtoBuf.Grpc.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddMudServices();

var handler = new GrpcWebHandler(GrpcWebMode.GrpcWebText, new HttpClientHandler());
var options = new GrpcChannelOptions
{
    HttpHandler = handler
};

var channel = GrpcChannel.ForAddress("https://localhost:5001", options);
channel.Intercept(new TestInterceptor());

var service = channel.CreateGrpcService<ITestService>();

Console.WriteLine(await service.Echo("Test"));

await builder.Build().RunAsync();