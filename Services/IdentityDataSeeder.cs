using BIsm2.Models; // your custom Users class
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BIsm2.Services
{
    public static class IdentityDataSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>()
                                        .CreateLogger("IdentityDataSeeder");

            string[] roleNames = { "User", "Seller", "Admin" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                    if (result.Succeeded)
                        logger.LogInformation($"Role '{roleName}' created successfully.");
                    else
                        logger.LogError($"Failed to create role '{roleName}': {string.Join(", ", result.Errors)}");
                }
            }
        }

        public static async Task SeedDefaultAdminAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<Users>>();
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>()
                                        .CreateLogger("IdentityDataSeeder");

            string adminEmail = "admin@example.com";
            string adminPassword = "Admin@12345!";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new Users
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    logger.LogInformation("Default admin user created and assigned to 'Admin' role.");
                }
                else
                {
                    foreach (var error in result.Errors)
                        logger.LogError($"Error code: {error.Code}, Description: {error.Description}");
                }
            }
            else
            {
                logger.LogInformation("Default admin user already exists.");
            }
        }
    }
}

