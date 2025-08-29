// Data/DatabaseInitializer.cs
using Microsoft.AspNetCore.Identity;
using TodoApp.API.Models;

namespace TodoApp.API.Data;

public static class DatabaseInitializer
{
    public static async Task Initialize(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // Create database if it doesn't exist
        context.Database.EnsureCreated();

        // Create roles
        string[] roleNames = { "Admin", "User" };
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Create admin user
        var adminEmail = "admin@todoapp.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createAdmin = await userManager.CreateAsync(admin, "Admin123!");
            if (createAdmin.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}