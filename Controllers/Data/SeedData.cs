using Microsoft.AspNetCore.Identity;
using WholesaleHub.Models;

namespace WholesaleHub.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager =
                serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager =
                serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles =
            {
                "Admin",
                "Warehouse Staff",
                "Accountant",
                "Customer"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            await CreateUser(
                userManager,
                "admin@wholesalehub.com",
                "Admin123!",
                "System Administrator",
                "Admin"
            );

            await CreateUser(
                userManager,
                "warehouse@wholesalehub.com",
                "Warehouse123!",
                "Warehouse Staff",
                "Warehouse Staff"
            );

            await CreateUser(
                userManager,
                "accountant@wholesalehub.com",
                "Accountant123!",
                "Accountant",
                "Accountant"
            );

            await CreateUser(
                userManager,
                "customer@wholesalehub.com",
                "Customer123!",
                "Sample Customer",
                "Customer"
            );
        }

        private static async Task CreateUser(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string fullName,
            string role)
        {
            var existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                return;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}