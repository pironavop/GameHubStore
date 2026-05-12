using GameHubStore.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GameHubStore.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Apply pending migrations automatically
            await context.Database.MigrateAsync();

            // 1. Create Roles
            string[] roles = { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Create Default Admin
            string adminEmail = "admin@gmail.com";
            string adminPassword = "Admin@123";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    FullName = "Admin User",
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Create Default Normal User
            string userEmail = "user@gmail.com";
            string userPassword = "User@123";

            var normalUser = await userManager.FindByEmailAsync(userEmail);

            if (normalUser == null)
            {
                normalUser = new ApplicationUser
                {
                    FullName = "Demo User",
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(normalUser, userPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(normalUser, "User");
                }
            }

            // 4. Create Categories
            if (!await context.Categories.AnyAsync())
            {
                context.Categories.AddRange(
                    new Category { Name = "Action", Description = "Fast-paced action games" },
                    new Category { Name = "Adventure", Description = "Story-based adventure games" },
                    new Category { Name = "RPG", Description = "Role-playing games" },
                    new Category { Name = "Racing", Description = "Car and bike racing games" },
                    new Category { Name = "Sports", Description = "Football, cricket, basketball and more" },
                    new Category { Name = "Shooter", Description = "FPS and third-person shooter games" },
                    new Category { Name = "Strategy", Description = "Tactical and strategy games" },
                    new Category { Name = "Simulation", Description = "Real-world simulation games" },
                    new Category { Name = "Horror", Description = "Scary and survival horror games" },
                    new Category { Name = "Open World", Description = "Large open-world exploration games" }
                );

                await context.SaveChangesAsync();
            }

            // 5. Create Platforms
            if (!await context.Platforms.AnyAsync())
            {
                context.Platforms.AddRange(
                    new Platform { Name = "PC" },
                    new Platform { Name = "PlayStation 5" },
                    new Platform { Name = "PlayStation 4" },
                    new Platform { Name = "Xbox Series X/S" },
                    new Platform { Name = "Xbox One" },
                    new Platform { Name = "Nintendo Switch" },
                    new Platform { Name = "Android" },
                    new Platform { Name = "iOS" }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}