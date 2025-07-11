using System.Security.Cryptography;
using System.Text;
using DKR.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;

namespace DKR.Core.Services;

public class SecurityService
{
    private readonly IAuditService _auditService;
    private readonly IConfiguration _configuration;

    public SecurityService(IAuditService auditService, IConfiguration configuration)
    {
        _auditService = auditService;
        _configuration = configuration;
    }

    // Field-Level Encryption (AES-256)
    public string EncryptSensitiveData(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        var key = GetEncryptionKey();
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Combine IV + encrypted data
        var result = new byte[aes.IV.Length + encryptedBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string DecryptSensitiveData(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText)) return encryptedText;

        try
        {
            var key = GetEncryptionKey();
            var encryptedData = Convert.FromBase64String(encryptedText);

            using var aes = Aes.Create();
            aes.Key = key;

            // Extract IV
            var iv = new byte[16];
            var encrypted = new byte[encryptedData.Length - 16];
            Buffer.BlockCopy(encryptedData, 0, iv, 0, 16);
            Buffer.BlockCopy(encryptedData, 16, encrypted, 0, encrypted.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var decryptedBytes = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch
        {
            return "[ENCRYPTED_DATA_ERROR]";
        }
    }

    // Input Validation & Sanitization
    public string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        // Remove dangerous characters
        input = input.Replace("<", "&lt;")
                    .Replace(">", "&gt;")
                    .Replace("\"", "&quot;")
                    .Replace("'", "&#x27;")
                    .Replace("&", "&amp;");

        // Remove SQL injection patterns
        var sqlPatterns = new[] { "--", "/*", "*/", "xp_", "sp_", "exec", "execute", "drop", "delete", "insert", "update" };
        foreach (var pattern in sqlPatterns)
        {
            input = input.Replace(pattern, "", StringComparison.OrdinalIgnoreCase);
        }

        return input.Trim();
    }

    public bool ValidateUUID(string uuid)
    {
        if (string.IsNullOrEmpty(uuid)) return false;

        // KL-YYYY-NNNN Format validieren
        var pattern = @"^KL-\d{4}-\d{4}$";
        return System.Text.RegularExpressions.Regex.IsMatch(uuid, pattern);
    }

    public bool ValidatePostalCode(string postalCode)
    {
        if (string.IsNullOrEmpty(postalCode)) return true; // Optional

        // Deutsche PLZ: 5 Ziffern
        var pattern = @"^\d{5}$";
        return System.Text.RegularExpressions.Regex.IsMatch(postalCode, pattern);
    }

    // Security Headers
    public void ConfigureSecurityHeaders(IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            // HSTS (HTTP Strict Transport Security)
            context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");

            // Content Security Policy - enhanced for camera access
            context.Response.Headers.Add("Content-Security-Policy", 
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data: blob:; " +
                "media-src 'self' blob:; " +  // Added for camera/barcode scanner
                "connect-src 'self'; " +
                "font-src 'self'; " +
                "frame-ancestors 'none'");

            // X-Frame-Options
            context.Response.Headers.Add("X-Frame-Options", "DENY");

            // X-Content-Type-Options
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

            // X-XSS-Protection
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

            // Referrer Policy
            context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

            // Remove Server Header
            context.Response.Headers.Remove("Server");

            await next();
        });
    }

    // Rate Limiting (einfache Version)
    private static readonly Dictionary<string, List<DateTime>> _requestLog = new();
    private static readonly object _lock = new object();

    public bool IsRateLimited(string clientIp, int maxRequests = 100, int timeWindowMinutes = 1)
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            var windowStart = now.AddMinutes(-timeWindowMinutes);

            if (!_requestLog.ContainsKey(clientIp))
            {
                _requestLog[clientIp] = new List<DateTime>();
            }

            var requests = _requestLog[clientIp];
            
            // Remove old requests
            requests.RemoveAll(r => r < windowStart);
            
            // Check if limit exceeded
            if (requests.Count >= maxRequests)
            {
                return true; // Rate limited
            }

            // Add current request
            requests.Add(now);
            return false;
        }
    }

    // Password Security
    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var salt = GenerateSalt();
        var saltedPassword = password + salt;
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        
        return Convert.ToBase64String(hashBytes) + ":" + Convert.ToBase64String(salt);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            var parts = hashedPassword.Split(':');
            if (parts.Length != 2) return false;

            var hash = parts[0];
            var salt = Convert.FromBase64String(parts[1]);
            
            using var sha256 = SHA256.Create();
            var saltedPassword = password + Encoding.UTF8.GetString(salt);
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            var computedHash = Convert.ToBase64String(hashBytes);

            return hash == computedHash;
        }
        catch
        {
            return false;
        }
    }

    private byte[] GenerateSalt()
    {
        var salt = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }

    private byte[] GetEncryptionKey()
    {
        var key = _configuration["Security:EncryptionKey"];
        if (string.IsNullOrEmpty(key))
        {
            // Fallback für Demo - in Produktion aus Key Vault
            key = "DKR-System-2024-Encryption-Key-32B!";
        }

        return Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
    }

    // Session Security
    public async Task LogSecurityEventAsync(string eventType, string description, string? userId = null)
    {
        await _auditService.LogAsync($"Security:{eventType}", "Security", userId ?? "System", description);
    }

    // GDPR Data Masking
    public string MaskPersonalData(string data, bool isFullAccess = false)
    {
        if (isFullAccess || string.IsNullOrEmpty(data)) return data;

        if (data.Length <= 2) return "***";
        
        return data.Substring(0, 2) + new string('*', data.Length - 2);
    }
}