using DKR.Core.Entities;
using DKR.Core.Interfaces;
using DKR.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DKR.Web.Auth;
using Microsoft.AspNetCore.Components.Authorization;

public class RealIdentityProvider : IIdentityProvider
{
    private readonly DKRDbContext _db;
    private readonly IPasswordHasher<User> _hasher;
    private readonly InMemoryAuthStateProvider _signInManager;
    public RealIdentityProvider(DKRDbContext db,
     IPasswordHasher<User> hasher,
     AuthenticationStateProvider authProvider)
    {
        _db = db;
        _hasher = hasher;

        _signInManager = authProvider as InMemoryAuthStateProvider
        ?? throw new InvalidOperationException("Expected InMemoryAuthStateProvider.");
    }

    public async Task<AuthenticationResult> AuthenticateAsync(string username, string password)
    {
        var user = await _db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                                  .FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return new AuthenticationResult { Success = false };

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Success)
        {
          var userInfo=  await GetUserInfoAsync(user.Id);
            await _signInManager.SignInAsync(userInfo);

            return new AuthenticationResult
            {
                Success = true,
                UserId = user.Id,
                RequiresTwoFactor = false
            };
        }
        return new AuthenticationResult { Success = false };
    }

    public Task<UserInfo> GetUserInfoAsync(string userId)
    {
        var user = _db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                            .FirstOrDefault(u => u.Id == userId);

        if (user == null) throw new Exception("User not found");

        var userInfo = new UserInfo
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
        };

        return Task.FromResult(userInfo);
    }

    public Task<IEnumerable<string>> GetUserRolesAsync(string userId)
    {
        var roles = _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role.Name)
            .ToList();

        return Task.FromResult<IEnumerable<string>>(roles);
    }

    public async Task<bool> CreateUserAsync(UserCreationRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Username == request.Username))
            return false;

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _hasher.HashPassword(null!, request.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateUserAsync(string userId, UserUpdateRequest request)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return false;

        user.Email = request.Email ?? user.Email;
        if (!string.IsNullOrEmpty(request.Password))
            user.PasswordHash = _hasher.HashPassword(user, request.Password);

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return false;

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignRoleAsync(string userId, string role)
    {
        var user = await _db.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Id == userId);
        var roleEntity = await _db.Roles.FirstOrDefaultAsync(r => r.Name == role);

        if (user == null || roleEntity == null)
            return false;

        if (user.UserRoles.Any(ur => ur.RoleId == roleEntity.Id))
            return true; // Already assigned

        user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleEntity.Id });
        await _db.SaveChangesAsync();
        return true;
    }

    public Task<bool> ValidateTwoFactorAsync(string userId, string code)
    {
        // Optional for now
        return Task.FromResult(true);
    }
}