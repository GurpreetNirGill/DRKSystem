using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using DKR.Core.Interfaces;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Text.Json;

namespace DKR.Web.Auth;

public class CookieAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
        return Task.FromResult(new AuthenticationState(user));
    }

    // Call this after login/logout to update Blazor UI
    public void NotifyAuthStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
    public async Task SignInAsync(ClaimsPrincipal principal, AuthenticationProperties properties)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        }
    }
}
 
public sealed class InMemoryAuthStateProvider : AuthenticationStateProvider
{
    private const string StorageKey = "dkr.auth";
    private readonly ProtectedLocalStorage _store;

    private ClaimsPrincipal _principal = new(new ClaimsIdentity());
    private bool _initialised;

    public InMemoryAuthStateProvider(ProtectedLocalStorage store) => _store = store;

    /* Called by Blazor frame‑work */
    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        Task.FromResult(new AuthenticationState(_principal));

    /* -------- PUBLIC API -------- */
    public async Task SignInAsync(UserInfo info)
    {
        _principal = BuildPrincipal(info);
        await _store.SetAsync(StorageKey, JsonSerializer.Serialize(info));
        Notify();
    }

    public async Task SignOutAsync()
    {
        _principal = new ClaimsPrincipal(new ClaimsIdentity());
        await _store.DeleteAsync(StorageKey);
        Notify();
    }

    /// <summary>Call once from OnAfterRenderAsync to load storage.</summary>
    public async Task EnsureInitializedAsync()
    {
        if (_initialised) return;
        _initialised = true;

        var stored = await _store.GetAsync<string>(StorageKey);
        if (stored.Success && !string.IsNullOrWhiteSpace(stored.Value))
        {
            var info = JsonSerializer.Deserialize<UserInfo>(stored.Value,
                         new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (info is not null)
            {
                _principal = BuildPrincipal(info);
                Notify();
            }
        }
    }

    /* Helpers */
    private void Notify() =>
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_principal)));

    private static ClaimsPrincipal BuildPrincipal(UserInfo u)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, u.UserId),
            new(ClaimTypes.Name,           u.Username),
            new(ClaimTypes.Email,          u.Email ?? "")
        };
        claims.AddRange(u.Roles.Select(r => new Claim(ClaimTypes.Role, r)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "InMemory"));
    }

}
