namespace DKR.Core.Interfaces;

public interface IIdentityProvider
{
    Task<AuthenticationResult> AuthenticateAsync(string username, string password);
    Task<bool> ValidateTwoFactorAsync(string userId, string code);
    Task<UserInfo> GetUserInfoAsync(string userId);
    Task<bool> CreateUserAsync(UserCreationRequest request);
    Task<bool> UpdateUserAsync(string userId, UserUpdateRequest request);
    Task<bool> DeleteUserAsync(string userId);
    Task<IEnumerable<string>> GetUserRolesAsync(string userId);
    Task<bool> AssignRoleAsync(string userId, string role);
}

public class AuthenticationResult
{
    public bool Success { get; set; }
    public string? UserId { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool RequiresTwoFactor { get; set; }
}

public class UserInfo
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? TenantId { get; set; }
    public string? FacilityId { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class UserCreationRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? TenantId { get; set; }
    public string? FacilityId { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class UserUpdateRequest
{
    public string? Email { get; set; }
    public string Password { get; set; } = string.Empty;
}