using DKR.Core.Entities;
using DKR.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DKRDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        // Ensure the database is created
        dbContext.Database.EnsureCreated();

        // Check if users already exist
        if (!dbContext.Users.Any())
        {
            var role = new Role { Name = "Admin" };

            var user = new User
            {
                Username = "dkradmin",
                Email = "admin@example.com",
            };

            user.PasswordHash = hasher.HashPassword(user, "Admin@123");

            var userRole = new UserRole
            {
                User = user,
                Role = role
            };

            dbContext.Roles.Add(role);
            dbContext.Users.Add(user);
            dbContext.UserRoles.Add(userRole);

            dbContext.SaveChanges();
        }
    }
}
