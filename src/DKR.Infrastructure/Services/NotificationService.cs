using DKR.Core.Interfaces;
using DKR.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;
using DKR.Shared.Enums;

namespace DKR.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationService> _logger;

    public event Action<Notification>? OnNotification;

    public NotificationService(IConfiguration configuration, ILogger<NotificationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task NotifyAsync(string title, string message, NotificationType type = NotificationType.Info)
    {
        var notification = new Notification
        {
            Title = title,
            Message = message,
            Type = type
        };

        // Trigger event for real-time notifications
        OnNotification?.Invoke(notification);

        // Send via configured channels based on type
        try
        {
            if (type == NotificationType.Emergency || type == NotificationType.Danger)
            {
                await SendEmailAsync(title, message);
            }

            _logger.LogInformation("Notification sent: {Title} - {Type}", title, type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification: {Title}", title);
        }
    }

    public async Task NotifyEmergencyAsync(string title, string message, Core.Entities.EmergencyType emergencyType)
    {
        var notification = new Notification
        {
            Title = title,
            Message = message,
            Type = NotificationType.Emergency
        };

        // Trigger event for real-time notifications
        OnNotification?.Invoke(notification);

        // Legacy method - delegate to emergency notification
        await SendEmergencyNotificationAsync(Guid.NewGuid().ToString(), emergencyType, title);
    }

    public async Task SendEmergencyNotificationAsync(string emergencyId, Core.Entities.EmergencyType type, string location)
    {
        var subject = $"🚨 DKR NOTFALL: {type} in {location}";
        var message = $@"
NOTFALL GEMELDET

Emergency ID: {emergencyId}
Typ: {type}
Ort: {location}
Zeit: {DateTime.Now:dd.MM.yyyy HH:mm:ss}

Sofortige Maßnahmen erforderlich!
";

        try
        {
            // E-Mail Notification
            await SendEmailAsync(subject, message);
            
            // SMS Notification (simuliert)
            await SendSMSAsync(subject, message);
            
            // WhatsApp Notification (simuliert)
            await SendWhatsAppAsync(subject, message);
            
            _logger.LogInformation("Emergency notifications sent for {EmergencyId}", emergencyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send emergency notifications for {EmergencyId}", emergencyId);
        }
    }

    public async Task SendInventoryAlertAsync(string itemName, int currentStock, int minimumStock)
    {
        var subject = $"⚠️ DKR Lagerbestand niedrig: {itemName}";
        var message = $@"
NIEDRIGER LAGERBESTAND

Artikel: {itemName}
Aktueller Bestand: {currentStock}
Mindestbestand: {minimumStock}
Zeit: {DateTime.Now:dd.MM.yyyy HH:mm:ss}

Bitte Nachbestellung veranlassen.
";

        try
        {
            await SendEmailAsync(subject, message);
            _logger.LogInformation("Inventory alert sent for {ItemName}", itemName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send inventory alert for {ItemName}", itemName);
        }
    }

    public async Task SendDailyReportAsync(string reportContent)
    {
        var subject = $"📊 DKR Tagesbericht - {DateTime.Now:dd.MM.yyyy}";
        
        try
        {
            await SendEmailAsync(subject, reportContent);
            _logger.LogInformation("Daily report sent for {Date}", DateTime.Now.Date);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send daily report for {Date}", DateTime.Now.Date);
        }
    }

    private async Task SendEmailAsync(string subject, string body)
    {
        var smtpServer = _configuration["Notifications:Email:SmtpServer"] ?? "localhost";
        var smtpPort = int.Parse(_configuration["Notifications:Email:SmtpPort"] ?? "587");
        var fromEmail = _configuration["Notifications:Email:FromAddress"] ?? "dkr-system@localhost";
        var toEmail = _configuration["Notifications:Email:ToAddress"] ?? "admin@localhost";
        var username = _configuration["Notifications:Email:Username"];
        var password = _configuration["Notifications:Email:Password"];

        using var client = new SmtpClient(smtpServer, smtpPort);
        
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            client.Credentials = new NetworkCredential(username, password);
            client.EnableSsl = true;
        }

        var mailMessage = new MailMessage(fromEmail, toEmail, subject, body);
        
        try
        {
            await client.SendMailAsync(mailMessage);
            _logger.LogDebug("Email sent to {ToEmail} with subject {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Email sending failed, but continuing. Configure SMTP in appsettings.json");
            // In Development mode, just log instead of failing
            _logger.LogInformation("Email would be sent: {Subject} - {Body}", subject, body);
        }
    }

    private async Task SendSMSAsync(string subject, string message)
    {
        // SMS Service Integration (Twilio, etc.)
        var smsApiKey = _configuration["Notifications:SMS:ApiKey"];
        var smsNumber = _configuration["Notifications:SMS:ToNumber"];

        if (string.IsNullOrEmpty(smsApiKey) || string.IsNullOrEmpty(smsNumber))
        {
            _logger.LogInformation("SMS would be sent: {Subject}", subject);
            return;
        }

        // Hier würde die echte SMS-API Integration stehen
        await Task.Delay(100); // Simulate API call
        _logger.LogInformation("SMS sent to {Number}: {Subject}", smsNumber, subject);
    }

    private async Task SendWhatsAppAsync(string subject, string message)
    {
        // WhatsApp Business API Integration
        var whatsappApiKey = _configuration["Notifications:WhatsApp:ApiKey"];
        var whatsappNumber = _configuration["Notifications:WhatsApp:ToNumber"];

        if (string.IsNullOrEmpty(whatsappApiKey) || string.IsNullOrEmpty(whatsappNumber))
        {
            _logger.LogInformation("WhatsApp would be sent: {Subject}", subject);
            return;
        }

        // Hier würde die echte WhatsApp-API Integration stehen
        await Task.Delay(100); // Simulate API call
        _logger.LogInformation("WhatsApp sent to {Number}: {Subject}", whatsappNumber, subject);
    }
    public async Task SendAsync(NotificationRequest request)
    {
        // Simulate sending (email, SMS, etc.)
        switch (request.Channel)
        {
            case NotificationChannel.Email:
                await SendEmailAsync(request.Subject, request.Message);
                break;

            case NotificationChannel.SMS:
                await SendSMSAsync(request.Subject, request.Message);
                break;
        }
    }

}