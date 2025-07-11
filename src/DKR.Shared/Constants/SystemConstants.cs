using DKR.Shared.Enums;

namespace DKR.Shared.Constants;

public static class SystemConstants
{
    public const int MaxSessionDurationMinutes = 30;
    public const int DataRetentionYears = 10;
    public const string UUIDFormat = "KL-{0:yyyy}-{1:D4}";
    
    public static class Roles
    {
        public const string Administrator = "Administrator";
        public const string Fachkraft = "Fachkraft";
        public const string Sozialarbeiter = "Sozialarbeiter";
        public const string ReadOnly = "ReadOnly";
        public const string TenantAdmin = "TenantAdmin";
        public const string SystemAdmin = "SystemAdmin";
    }
    
    public static class Policies
    {
        public const string RequireAdminRole = "RequireAdminRole";
        public const string RequireStaffRole = "RequireStaffRole";
        public const string RequireTenantAccess = "RequireTenantAccess";
    }
    
    public static class SignalRHubs
    {
        public const string Dashboard = "/dashboardHub";
        public const string Sessions = "/sessionHub";
        public const string Emergency = "/emergencyHub";
    }
    
    public static class CacheKeys
    {
        public const string ActiveSessions = "active_sessions";
        public const string TenantConfig = "tenant_config_{0}";
        public const string UserRoles = "user_roles_{0}";
    }

    public static string GetSessionStatusText(SessionStatus status) => status switch
    {
        SessionStatus.Active => "Aktiv",
        SessionStatus.Emergency => "Notfall",
        SessionStatus.Monitoring => "Überwachung",
        SessionStatus.Waiting => "Wartend",
        SessionStatus.Pause => "Pausieren",
        SessionStatus.Completed => "Vollendet",
        SessionStatus.Cancelled => "Abgesagt",
        _ => "Unbekannt"
    };
}