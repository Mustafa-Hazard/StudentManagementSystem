using Microsoft.AspNetCore.Identity;
using SMS.Models.Entities; // Aapka custom user model yahan se aayega
using System;
using System.Threading.Tasks;

namespace SMS.Data // Namespace aapke project ke mutabiq theek kar diya hai
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            // IdentityUser ki jagah ApplicationUser taake FullName save ho sake
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Ensure Roles exist
            string[] roles = { "Admin", "Teacher", "Student" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Create the Default Admin (User provided details)
            var adminEmail = "nmeisSMS@SCD.com";
            var adminPassword = "nmeisSCD0@";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Mustafa Muhammad Iqbal", //
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, adminPassword);

                if (!result.Succeeded)
                {
                    Console.WriteLine("Admin user creation failed:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"- {error.Description}");
                    }
                    return;
                }

                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}