using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using GrpcAuth.Contracts.Models.Wrapper;
using GrpcAuth.Server.Configuration;
using GrpcAuth.Server.Contexts;
using GrpcAuth.Server.Models.Identity;
using GrpcAuth.Server.Permission;
using GrpcAuth.Server.Seeder;
using GrpcAuth.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MudBlazor.Services;
using ProtoBuf.Grpc.Server;

namespace GrpcAuth.Server;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration) => _configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors();

        services.Configure<ApplicationConfiguration>(_configuration.GetSection(nameof(ApplicationConfiguration)));
                
        services.AddRazorPages();
        services.AddCodeFirstGrpc();
        
        services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(_configuration.GetConnectionString("DefaultConnection")));
        services.AddTransient<IDatabaseSeeder, DatabaseSeeder>();
        
        services
            .AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>()
            .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>()
            .AddIdentity<ApplicationUser, ApplicationRole>(
                options =>
                {
                    options.Password.RequiredLength = 6;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.User.RequireUniqueEmail = true;
                })
            .AddEntityFrameworkStores<ApplicationContext>()
            .AddDefaultTokenProviders();
        
        var key = Encoding.UTF8.GetBytes(_configuration.GetSection(nameof(ApplicationConfiguration)).Get<ApplicationConfiguration>().Secret!);
        services.AddAuthentication(authentication =>
        {
            authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(bearer =>
        {
            bearer.RequireHttpsMetadata = false;
            bearer.SaveToken = true;
            bearer.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                RoleClaimType = ClaimTypes.Role,
                ClockSkew = TimeSpan.Zero
            };

            bearer.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = async c =>
                {
                    if (c.Exception is SecurityTokenExpiredException)
                    {
                        c.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                        c.Response.ContentType = "application/json";
                        var result = JsonSerializer.Serialize(await Result.FailAsync("The Token is expired."));
                        await c.Response.WriteAsync(result);
                    }
                    else
                    {
                        c.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                        c.Response.ContentType = "application/json";
                        var result = JsonSerializer.Serialize(await Result.FailAsync("An unhandled error has occurred."));
                        await c.Response.WriteAsync(result);
                    }
                },
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                        context.Response.ContentType = "application/json";
                        var result = JsonSerializer.Serialize(await Result.FailAsync("You are not Authorized."));
                        await context.Response.WriteAsync(result);
                    }
                },
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                    context.Response.ContentType = "application/json";
                    var result = JsonSerializer.Serialize(await Result.FailAsync("You are not authorized to access this resource."));
                    await context.Response.WriteAsync(result);
                }
            };
        });
        services.AddAuthorization();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseGrpcWeb(new GrpcWebOptions
        {
            DefaultEnabled = true
        });
        app.UseCors();

        app.UseEndpoints(
            endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapGrpcService<TokenService>();
                endpoints.MapGrpcService<UserService>();
                endpoints.MapFallbackToFile("index.html");
            });
        
        using var scope = app.ApplicationServices.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
        seeder.Seed();
    }
}