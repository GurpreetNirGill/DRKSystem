using DKR.Core.Entities;
using DKR.Core.Services;

namespace DKR.Core.Interfaces;

public interface IAuditService
{
    Task LogAsync(string action, string entityType, string entityId, object? oldValues = null, object? newValues = null);
    Task LogAsync(string action, string entityType, string entityId, string description);
    Task<bool> VerifyIntegrityAsync();
    Task<AuditTrailReport> GenerateAuditReportAsync(DateTime from, DateTime to, string? entityType = null, string? userId = null);
    Task<ComplianceReport> GenerateComplianceReportAsync(DateTime from, DateTime to);
}

public interface IAuditRepository
{
    Task<AuditLog> CreateAsync(AuditLog auditLog);
    Task<AuditLog?> GetLastAuditLogAsync();
    Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync();
    Task<IEnumerable<AuditLog>> GetAuditLogsByDateRangeAsync(DateTime from, DateTime to);
    Task<IEnumerable<AuditLog>> GetAuditLogsByEntityAsync(string entityType, string entityId);
    Task<IEnumerable<AuditLog>> GetAuditLogsByUserAsync(string userId);
    Task<AuditLog> GetByIdAsync(string id);
    Task<bool> DeleteAsync(string id);
    Task<AuditLog> UpdateAsync(AuditLog auditLog);
}

public interface ICurrentUserService
{
    string GetCurrentUserId();
    string? GetCurrentTenantId();
    string GetCurrentUserName();
    List<string> GetCurrentUserRoles();
}

public interface IEmailService
{
    Task SendEmergencyEmailAsync(List<string> recipients, string subject, string message);
    Task SendReportEmailAsync(string recipient, string subject, string htmlContent, byte[]? attachment = null);
}

public interface ISMSService
{
    Task SendEmergencySMSAsync(List<string> phoneNumbers, string message);
    Task SendReminderSMSAsync(string phoneNumber, string message);
}

public interface IWhatsAppService
{
    Task SendEmergencyWhatsAppAsync(List<string> phoneNumbers, string message);
    Task SendAppointmentReminderAsync(string phoneNumber, string message);
}

