using BestStoreMVC.Models;
using Microsoft.AspNetCore.Identity;

namespace BestStoreMVC.Services
{
    public class DatabaseInitializer
    {

        // using the AspNetCore.Identity features and func for User/RoleManager
        public static async Task SeedDataAsync(UserManager<ApplicationUser>? userManager, RoleManager<IdentityRole>? roleManager)
        {
            if (userManager == null || roleManager == null)
            {
                return;
            }
            var exists = await roleManager.RoleExistsAsync("admin");
            if (!exists)
            {
                await roleManager.CreateAsync(new IdentityRole("admin"));
            }

            exists = await roleManager.RoleExistsAsync("seller");
            if (!exists)
            {
                await roleManager.CreateAsync(new IdentityRole("seller"));
            }

            exists = await roleManager.RoleExistsAsync("client");
            if (!exists)
            {
                await roleManager.CreateAsync(new IdentityRole("client"));
            }

            var admitUsers = await userManager.GetUsersInRoleAsync("admin");
            if (admitUsers.Any())
            {
                return;
            }


            var user = new ApplicationUser()
            {
                FirstName = "Admin",
                LastName = "Admin",
                UserName = "admin@admin.com",
                Email = "admin@admin.com",
                CreateAt = DateTime.Now,
            };

            string initialPassword = "admint123";
            var result = await userManager.CreateAsync(user, initialPassword);
            if (result.Succeeded)
            {
                // set the user role
                await userManager.AddToRoleAsync(user, "Admin");
                Console.WriteLine("Admin user created successfully! Please update the initial password.");
                Console.WriteLine("Email: " + user.Email);
                Console.WriteLine("Initial password: " + initialPassword);
            }
        }

    }
}
