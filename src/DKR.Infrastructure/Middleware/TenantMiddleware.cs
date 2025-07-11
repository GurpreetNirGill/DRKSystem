using Microsoft.AspNetCore.Http;

namespace DKR.Infrastructure.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Tenant aus Subdomain extrahieren: hamburg.dkr-system.com
        var tenant = ExtractTenantFromSubdomain(context.Request.Host);
        
        // Fallback: Header X-Tenant-ID (für API calls)
        if (string.IsNullOrEmpty(tenant))
        {
            tenant = context.Request.Headers["X-Tenant-ID"].FirstOrDefault();
        }
        
        // Tenant im HttpContext speichern
        if (!string.IsNullOrEmpty(tenant))
        {
            context.Items["TenantId"] = tenant;
        }
        
        await _next(context);
    }

    private string? ExtractTenantFromSubdomain(HostString host)
    {
        var subdomain = host.Host.Split('.').FirstOrDefault();
        
        // Ignoriere www und localhost
        if (subdomain == "www" || subdomain == "localhost" || string.IsNullOrEmpty(subdomain))
        {
            return null;
        }
        
        return subdomain;
    }
}