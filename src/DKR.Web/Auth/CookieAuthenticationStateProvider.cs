using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using DKR.Core.Interfaces;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Text.Json;
using System.Security.Cryptography;

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
    private const string StorageKey = "dkr.user";
    private readonly ProtectedLocalStorage _store;

    private ClaimsPrincipal _principal = new(new ClaimsIdentity());
    private bool _initialised;

    public InMemoryAuthStateProvider(ProtectedLocalStorage store) => _store = store;

    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        Task.FromResult(new AuthenticationState(_principal));

    /* -------- PUBLIC API -------- */

    public async Task SignInAsync(UserInfo info)
    {
        _principal = BuildPrincipal(info);

        try
        {
            var json = JsonSerializer.Serialize(info);
            await _store.SetAsync(StorageKey, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Fehler beim Speichern im LocalStorage: {ex.Message}");
        }

        Notify();
    }

    public async Task SignOutAsync()
    {
        _principal = new ClaimsPrincipal(new ClaimsIdentity());

        try
        {
            await _store.DeleteAsync(StorageKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Fehler beim Löschen aus LocalStorage: {ex.Message}");
        }

        Notify();
    }

    /// <summary>Call once from OnAfterRenderAsync to load storage.</summary>
    public async Task EnsureInitializedAsync()
    {
        if (_initialised) return;
        _initialised = true;

        try
        {
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
        catch (CryptographicException)
        {
            // Schlüssel nicht mehr gültig – Daten entfernen
            Console.WriteLine("⚠️ Gespeicherte Auth-Daten konnten nicht entschlüsselt werden. Sie wurden gelöscht.");
            try
            {
                await _store.DeleteAsync(StorageKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Fehler beim Löschen beschädigter Daten: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Unerwarteter Fehler beim Initialisieren des Auth-Status: {ex.Message}");
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
            new(ClaimTypes.Name, u.Username),
            new(ClaimTypes.Email, u.Email ?? "")
        };

        claims.AddRange(u.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "InMemory"));
    }

    public async Task<UserInfo?> GetStoredUserAsync()
    {
        try
        {
            var stored = await _store.GetAsync<string>(StorageKey);

            if (stored.Success && !string.IsNullOrWhiteSpace(stored.Value))
            {
                var user = JsonSerializer.Deserialize<UserInfo>(stored.Value, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return user;
            }
        }
        catch (CryptographicException)
        {
            await _store.DeleteAsync(StorageKey);
            Console.WriteLine("⚠️ Gespeicherte Auth-Daten konnten nicht entschlüsselt werden. Sie wurden gelöscht.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Fehler beim Lesen gespeicherter Benutzerinformationen: {ex.Message}");
        }

        return null;
    }

}

