namespace GrpcAuth.Server;

internal static class Program
{
    private static async Task Main() =>
        await Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(
                builder =>
                {
                    builder.UseStaticWebAssets();
                    builder.UseStartup<Startup>();
                })
            .Build()
            .RunAsync();
}