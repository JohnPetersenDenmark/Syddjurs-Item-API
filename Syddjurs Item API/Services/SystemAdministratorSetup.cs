using Microsoft.AspNetCore.Identity;
using Syddjurs_Item_API.Models;

namespace Syddjurs_Item_API.Services
{
    public static class SystemAdministratorSetup
    {
        public static async Task EnsureAdminUserAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            string adminRole = "Administrator";
            string managerRole = "Manager";
            string userRole = "User";
            string adminUsername = "Administrator";
            string adminPassword = "Admin123!"; // Use a secure password in production

            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            if (!await roleManager.RoleExistsAsync(managerRole))
            {
                await roleManager.CreateAsync(new IdentityRole(managerRole));
            }

            if (!await roleManager.RoleExistsAsync(userRole))
            {
                await roleManager.CreateAsync(new IdentityRole(userRole));
            }

            var adminUser = await userManager.FindByNameAsync(adminUsername);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminUsername,
                    Email = "admin@placeholder.local", // still needed
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, adminRole);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to create admin user: {errors}");
                }
            }
        }
    }
}
