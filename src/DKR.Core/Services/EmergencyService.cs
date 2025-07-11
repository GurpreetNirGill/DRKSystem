using DKR.Core.Entities;
using DKR.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DKR.Core.Services;

public class EmergencyService
{
    private readonly IEmergencyRepository _emergencyRepository;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly ISMSService _smsService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IAuditService _auditService;

    public EmergencyService(
        IEmergencyRepository emergencyRepository,
        INotificationService notificationService,
        IEmailService emailService,
        ISMSService smsService,
        IWhatsAppService whatsAppService,
        IAuditService auditService)
    {
        _emergencyRepository = emergencyRepository;
        _notificationService = notificationService;
        _emailService = emailService;
        _smsService = smsService;
        _whatsAppService = whatsAppService;
        _auditService = auditService;
    }

    public async Task<EmergencyEvent> ReportEmergencyAsync(EmergencyEvent emergencyEvent)
    {
        // 1. Notfall in Datenbank speichern
        var savedEmergency = await _emergencyRepository.CreateAsync(emergencyEvent);

        // 2. Audit-Log erstellen
        await _auditService.LogAsync("EmergencyReported", "EmergencyEvent", savedEmergency.Id, 
            $"Notfall gemeldet: {emergencyEvent.Type} in {emergencyEvent.Room}");

        // 3. Automatische Benachrichtigungen senden
        await SendAutomaticNotificationsAsync(savedEmergency);

        // 4. Live-Notification im System
        await _notificationService.NotifyAsync("🚨 NOTFALL", 
            $"Notfall in {emergencyEvent.Room}: {emergencyEvent.Type}", 
            NotificationType.Emergency);

        return savedEmergency;
    }

    private async Task SendAutomaticNotificationsAsync(EmergencyEvent emergency)
    {
        var timestamp = emergency.OccurredAt.ToString("dd.MM.yyyy HH:mm:ss");
        var message = CreateEmergencyMessage(emergency, timestamp);

        try
        {
            // Parallele Benachrichtigungen
            var tasks = new List<Task>();

            // E-Mail an Behörden
            tasks.Add(_emailService.SendEmergencyEmailAsync(
                GetAuthorityEmails(), 
                "🚨 NOTFALL - DKR Hamburg-Altona", 
                message));

            // SMS an Rettungsdienst
            tasks.Add(_smsService.SendEmergencySMSAsync(
                GetEmergencyNumbers(), 
                $"NOTFALL DKR: {emergency.Type} in {emergency.Room}. Adresse: Große Bergstraße 267, 22767 Hamburg"));

            // WhatsApp an Leitungsteam
            tasks.Add(_whatsAppService.SendEmergencyWhatsAppAsync(
                GetManagementNumbers(), 
                CreateWhatsAppMessage(emergency, timestamp)));

            // Automatischer Notruf 112 (simuliert)
            if (emergency.Type == EmergencyType.Overdose || 
                emergency.Type == EmergencyType.RespiratoryArrest)
            {
                tasks.Add(Call112Async(emergency));
            }

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            await _auditService.LogAsync("EmergencyNotificationFailed", "EmergencyEvent", emergency.Id, 
                $"Fehler bei Notfall-Benachrichtigung: {ex.Message}");
            
            // Fallback: Manuelle Benachrichtigung erforderlich
            await _notificationService.NotifyAsync("⚠️ Benachrichtigung fehlgeschlagen", 
                "Automatische Notfall-Benachrichtigung fehlgeschlagen. Manuelle Alarmierung erforderlich!", 
                NotificationType.Emergency);
        }
    }

    private string CreateEmergencyMessage(EmergencyEvent emergency, string timestamp)
    {
        return $@"
🚨 NOTFALL-MELDUNG DKR HAMBURG-ALTONA 🚨

Zeitpunkt: {timestamp}
Art: {GetEmergencyTypeGerman(emergency.Type)}
Ort: {emergency.Room}
Klient: {emergency.ClientId}

ICD-10: {emergency.ICD10Code}

Durchgeführte Maßnahmen:
{string.Join("\n", emergency.ActionsPerformed.Select(a => "• " + a))}

Naloxon verabreicht: {(emergency.NaloxoneAdministered ? "JA" : "NEIN")}
Rettungsdienst alarmiert: {(emergency.EmergencyServicesCalled ? "JA" : "NEIN")}

Notizen:
{emergency.Notes}

Einrichtung: DKR Hamburg-Altona
Adresse: Große Bergstraße 267, 22767 Hamburg
Telefon: +49 40 123456789

Diese Meldung wurde automatisch generiert.
Notfall-ID: {emergency.Id}
";
    }

    private string CreateWhatsAppMessage(EmergencyEvent emergency, string timestamp)
    {
        return $@"🚨 *NOTFALL DKR* 🚨

*{GetEmergencyTypeGerman(emergency.Type)}* in {emergency.Room}
⏰ {timestamp}

👤 Klient: {emergency.ClientId}
💉 Naloxon: {(emergency.NaloxoneAdministered ? "✅ Verabreicht" : "❌ Nicht verabreicht")}
🚑 RTW: {(emergency.EmergencyServicesCalled ? "✅ Alarmiert" : "❌ Nicht alarmiert")}

📝 {emergency.Notes}

ID: {emergency.Id}";
    }

    private async Task Call112Async(EmergencyEvent emergency)
    {
        // In Produktion: Integration mit Notruf-System
        // Hier: Simulation für Demo
        await Task.Delay(1000);
        
        await _auditService.LogAsync("Emergency112Called", "EmergencyEvent", emergency.Id, 
            "Automatischer Notruf 112 ausgelöst");
        
        // Log für Compliance
        Console.WriteLine($"[EMERGENCY] Notruf 112 für Notfall {emergency.Id} ausgelöst");
    }

    private List<string> GetAuthorityEmails()
    {
        return new List<string>
        {
            "gesundheitsamt@hamburg.de",
            "drogenbeauftragte@hamburg.de",
            "sozialbehoerde@hamburg.de"
        };
    }

    private List<string> GetEmergencyNumbers()
    {
        return new List<string>
        {
            "+49 40 112112", // Rettungsleitstelle Hamburg
            "+49 40 123456789" // DKR Notfallnummer
        };
    }

    private List<string> GetManagementNumbers()
    {
        return new List<string>
        {
            "+49 171 1234567", // Dr. Maria Schmidt
            "+49 172 2345678", // Teamleitung
            "+49 173 3456789"  // Bereitschaftsdienst
        };
    }

    private string GetEmergencyTypeGerman(EmergencyType type)
    {
        return type switch
        {
            EmergencyType.Overdose => "Überdosierung",
            EmergencyType.Unconsciousness => "Bewusstlosigkeit",
            EmergencyType.Seizure => "Krampfanfall",
            EmergencyType.RespiratoryArrest => "Atemstillstand",
            EmergencyType.Injury => "Verletzung",
            EmergencyType.PsychiatricEmergency => "Psychischer Notfall",
            _ => "Unbekannter Notfall"
        };
    }

    public async Task<IEnumerable<EmergencyEvent>> GetEmergencyHistoryAsync(DateTime from, DateTime to)
    {
        return await _emergencyRepository.GetByDateRangeAsync(from, to);
    }

    public async Task<EmergencyStatistics> GetEmergencyStatisticsAsync(DateTime from, DateTime to)
    {
        var emergencies = await _emergencyRepository.GetByDateRangeAsync(from, to);
        
        return new EmergencyStatistics
        {
            TotalEmergencies = emergencies.Count(),
            OverdoseCount = emergencies.Count(e => e.Type == EmergencyType.Overdose),
            NaloxoneAdministrations = emergencies.Count(e => e.NaloxoneAdministered),
            ResponseTimeAverage = CalculateAverageResponseTime(emergencies),
            MostCommonLocation = emergencies.GroupBy(e => e.Room)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? "Unbekannt"
        };
    }

    private TimeSpan CalculateAverageResponseTime(IEnumerable<EmergencyEvent> emergencies)
    {
        // Placeholder - in Produktion würde man echte Response-Zeiten berechnen
        return TimeSpan.FromMinutes(3.5);
    }
    public async Task<IEnumerable<EmergencyEvent>> GetAllEmergencyEventsAsync()
    {
        return await _emergencyRepository.GetAllEmergencyEventsAsync();
    }
}

public class EmergencyStatistics
{
    public int TotalEmergencies { get; set; }
    public int OverdoseCount { get; set; }
    public int NaloxoneAdministrations { get; set; }
    public TimeSpan ResponseTimeAverage { get; set; }
    public string MostCommonLocation { get; set; } = string.Empty;
}