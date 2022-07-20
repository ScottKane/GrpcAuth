using GrpcAuth.Server.Contexts;
using GrpcAuth.Server.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace GrpcAuth.Server.Seeder;

public class DatabaseSeeder : IDatabaseSeeder
{
    private readonly ApplicationContext _db;
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public DatabaseSeeder(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ApplicationContext db,
        ILogger<DatabaseSeeder> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
        _logger = logger;
    }

    public void Seed()
    {
        AddAdministrator();
        AddBasicUser();
        _db.SaveChanges();
    }

    private void AddAdministrator()
    {
        Task.Run(
            async () =>
            {
                var adminRole = new ApplicationRole(
                    "Administrator",
                    "Administrator role with full permissions");
                var adminRoleInDb = await _roleManager.FindByNameAsync("Administrator");
                if (adminRoleInDb == null)
                {
                    await _roleManager.CreateAsync(adminRole);
                    adminRoleInDb = await _roleManager.FindByNameAsync("Administrator");
                    _logger.LogInformation("Seeded \"Administrator\" Role");
                }

                var adminUser = new ApplicationUser
                {
                    FirstName = "John",
                    LastName = "Smith",
                    Email = "john@email.co.uk",
                    UserName = "john@email.co.uk",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedOn = DateTime.Now,
                    IsActive = true
                };
                var adminUserInDb = await _userManager.FindByEmailAsync(adminUser.Email);
                if (adminUserInDb == null)
                {
                    await _userManager.CreateAsync(
                        adminUser,
                        "123Pa$$word!");
                    var result = await _userManager.AddToRoleAsync(
                        adminUser,
                        "Administrator");
                    if (result.Succeeded)
                        _logger.LogInformation("Seeded \"Administrator\" User");
                    else
                        foreach (var error in result.Errors)
                            if (error.Description != null)
                                _logger.LogError("{Description}", error.Description);
                }
            })
            .GetAwaiter()
            .GetResult();
    }

    private void AddBasicUser()
    {
        Task.Run(
            async () =>
            {
                var basicRole = new ApplicationRole(
                    "Basic",
                    "Basic role with default permissions");
                var basicRoleInDb = await _roleManager.FindByNameAsync("Basic");
                if (basicRoleInDb == null)
                {
                    await _roleManager.CreateAsync(basicRole);
                    _logger.LogInformation("Seeded \"Basic\" Role");
                }

                var basicUser = new ApplicationUser
                {
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane@email.co.uk",
                    UserName = "jane@email.co.uk",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedOn = DateTime.Now,
                    IsActive = true
                };
                var basicUserInDb = await _userManager.FindByEmailAsync(basicUser.Email);
                if (basicUserInDb == null)
                {
                    await _userManager.CreateAsync(
                        basicUser,
                        "123Pa$$word!");
                    await _userManager.AddToRoleAsync(
                        basicUser,
                        "Basic");
                    _logger.LogInformation("Seeded \"Basic\" User");
                }
            })
            .GetAwaiter()
            .GetResult();
    }
}