using DKR.Shared.Enums;

namespace DKR.Core.Entities;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Email { get; set; } = null!;
    public List<UserRole> UserRoles { get; set; } = new();
}

