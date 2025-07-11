namespace DKR.Core.Interfaces;

public interface IAuthorizationService
{
    bool IsInRole(string role);
    bool HasPermission(string permission);
    string GetCurrentUserId();
    Task<bool> CanAccessClientDataAsync(string clientId);
    Task<bool> CanPerformActionAsync(string action, string? entityId = null);
}

public static class Roles
{
    public const string Administrator = "Administrator";
    public const string Fachkraft = "Fachkraft";
    public const string Sozialarbeiter = "Sozialarbeiter";
    public const string ReadOnly = "ReadOnly";
    public const string TenantAdmin = "TenantAdmin";
    public const string SystemAdmin = "SystemAdmin";
}

public static class Permissions
{
    public const string CreateClient = "CreateClient";
    public const string ViewClient = "ViewClient";
    public const string EditClient = "EditClient";
    public const string DeleteClient = "DeleteClient";
    public const string StartSession = "StartSession";
    public const string EndSession = "EndSession";
    public const string HandleEmergency = "HandleEmergency";
    public const string ExportData = "ExportData";
    public const string ManageInventory = "ManageInventory";
    public const string ViewReports = "ViewReports";
    public const string SystemSettings = "SystemSettings";
}