using DKR.Shared.Enums;

namespace DKR.Core.Entities;

public class Role
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = null!;
    public List<UserRole> UserRoles { get; set; } = new();
}