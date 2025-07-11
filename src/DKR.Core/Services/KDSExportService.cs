using DKR.Core.Entities;
using DKR.Core.Interfaces;
using DKR.Shared.Enums;
using System.Text;
using System.Xml.Linq;

namespace DKR.Core.Services;

public class KDSExportService
{
    private readonly IClientRepository _clientRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IEmergencyRepository _emergencyRepository;
    private readonly IAuditService _auditService;

    public KDSExportService(
        IClientRepository clientRepository,
        ISessionRepository sessionRepository,
        IEmergencyRepository emergencyRepository,
        IAuditService auditService)
    {
        _clientRepository = clientRepository;
        _sessionRepository = sessionRepository;
        _emergencyRepository = emergencyRepository;
        _auditService = auditService;
    }

    public async Task<KDSExportResult> ExportKDS30Async(DateTime from, DateTime to, string facilityId = "HH-DKR-001")
    {
        try
        {
            // 1. Daten sammeln
            var sessions = await _sessionRepository.GetSessionsByDateRangeAsync(from, to);
            var clients = await GetUniqueClientsFromSessions(sessions);
            var emergencies = await _emergencyRepository.GetByDateRangeAsync(from, to);

            // 2. KDS 3.0 XML generieren
            var xmlContent = GenerateKDS30XML(sessions, clients, emergencies, facilityId, from, to);

            // 3. Validierung gegen KDS-Schema
            var validationResult = ValidateKDSSchema(xmlContent);

            // 4. Export-Datei erstellen
            var fileName = $"KDS30_Export_{facilityId}_{from:yyyyMMdd}_{to:yyyyMMdd}.xml";
            var fileContent = Encoding.UTF8.GetBytes(xmlContent);

            // 5. Audit-Log
            await _auditService.LogAsync("KDSExportGenerated", "Export", fileName, 
                $"KDS 3.0 Export erstellt: {sessions.Count()} Sessions, {clients.Count()} Klienten");

            return new KDSExportResult
            {
                Success = validationResult.IsValid,
                FileName = fileName,
                FileContent = fileContent,
                XmlContent = xmlContent,
                ValidationResult = validationResult,
                SessionCount = sessions.Count(),
                ClientCount = clients.Count(),
                EmergencyCount = emergencies.Count(),
                ExportedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            await _auditService.LogAsync("KDSExportFailed", "Export", $"{from:yyyyMMdd}-{to:yyyyMMdd}", 
                $"KDS Export fehlgeschlagen: {ex.Message}");
            
            return new KDSExportResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private string GenerateKDS30XML(
        IEnumerable<Session> sessions, 
        IEnumerable<Client> clients, 
        IEnumerable<EmergencyEvent> emergencies,
        string facilityId, 
        DateTime from, 
        DateTime to)
    {
        var kdsDocument = new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            new XElement("KDS30",
                new XAttribute("version", "3.0"),
                new XAttribute("xmlns", "http://www.suchthilfe.de/kds/3.0"),
                new XAttribute("facility", facilityId),
                new XAttribute("reportPeriod", $"{from:yyyy-MM-dd} bis {to:yyyy-MM-dd}"),
                new XAttribute("exportDate", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")),
                
                // Einrichtungsdaten
                GenerateFacilityData(facilityId),
                
                // Klientendaten
                GenerateClientData(clients),
                
                // Konsumvorgänge (Sessions)
                GenerateSessionData(sessions),
                
                // Notfälle
                GenerateEmergencyData(emergencies),
                
                // Statistiken
                GenerateStatistics(sessions, clients, emergencies)
            )
        );

        return kdsDocument.ToString();
    }

    private XElement GenerateFacilityData(string facilityId)
    {
        return new XElement("Einrichtung",
            new XElement("ID", facilityId),
            new XElement("Name", "Drogenkonsumraum Hamburg-Altona"),
            new XElement("Adresse",
                new XElement("Strasse", "Große Bergstraße 267"),
                new XElement("PLZ", "22767"),
                new XElement("Ort", "Hamburg"),
                new XElement("Bundesland", "Hamburg")
            ),
            new XElement("Kapazitaet", 5),
            new XElement("OeffnungsZeiten", "Mo-Fr 09:00-17:00"),
            new XElement("Zielgruppe", "Intravenös und inhalativ konsumierende Drogenabhängige")
        );
    }

    private XElement GenerateClientData(IEnumerable<Client> clients)
    {
        return new XElement("Klienten",
            clients.Select(client => new XElement("Klient",
                new XElement("UUID", client.UUID),
                new XElement("Geschlecht", GetKDSGender(client.Gender)),
                new XElement("Geburtsjahr", client.BirthYear),
                new XElement("PLZ", client.PostalCode ?? ""),
                new XElement("Hauptsubstanz", GetKDSSubstance(client.MainSubstance)),
                new XElement("Erstbesuch", client.FirstVisit ? "Ja" : "Nein"),
                new XElement("Behandlungshistorie", GetKDSTreatmentHistory(client.TreatmentHistory)),
                new XElement("ErsterBesuch", client.FirstVisitDate.ToString("yyyy-MM-dd")),
                new XElement("LetzterBesuch", client.LastVisitDate.ToString("yyyy-MM-dd")),
                new XElement("AnzahlSessions", client.Sessions.Count)
            ))
        );
    }

    private XElement GenerateSessionData(IEnumerable<Session> sessions)
    {
        return new XElement("Konsumvorgaenge",
            sessions.Select(session => new XElement("Konsumvorgang",
                new XElement("ID", session.Id),
                new XElement("KlientUUID", session.Client?.UUID ?? "Unbekannt"),
                new XElement("Datum", session.StartTime.ToString("yyyy-MM-dd")),
                new XElement("Uhrzeit", session.StartTime.ToString("HH:mm:ss")),
                new XElement("Dauer", session.Duration.TotalMinutes.ToString("F0")),
                new XElement("Raum", session.Room),
                new XElement("Substanz", GetKDSSubstance(session.Substance)),
                new XElement("Applikationsweg", GetKDSApplicationMethod(session.ApplicationMethod)),
                new XElement("Status", GetKDSSessionStatus(session.Status)),
                
                session.BloodPressure != null ? new XElement("Blutdruck", session.BloodPressure) : null,
                session.Pulse.HasValue ? new XElement("Puls", session.Pulse.Value) : null,
                !string.IsNullOrEmpty(session.Notes) ? new XElement("Bemerkungen", session.Notes) : null
            ))
        );
    }

    private XElement GenerateEmergencyData(IEnumerable<EmergencyEvent> emergencies)
    {
        return new XElement("Notfaelle",
            emergencies.Select(emergency => new XElement("Notfall",
                new XElement("ID", emergency.Id),
                new XElement("KlientUUID", emergency.ClientId),
                new XElement("Datum", emergency.OccurredAt.ToString("yyyy-MM-dd")),
                new XElement("Uhrzeit", emergency.OccurredAt.ToString("HH:mm:ss")),
                new XElement("Art", GetKDSEmergencyType(emergency.Type)),
                new XElement("Ort", emergency.Room),
                new XElement("ICD10", emergency.ICD10Code ?? ""),
                new XElement("NaloxonVerabreicht", emergency.NaloxoneAdministered ? "Ja" : "Nein"),
                emergency.NaloxoneDoses > 0 ? new XElement("NaloxonDosen", emergency.NaloxoneDoses) : null,
                new XElement("RettungsdienstAlarmiert", emergency.EmergencyServicesCalled ? "Ja" : "Nein"),
                new XElement("Massnahmen",
                    emergency.ActionsPerformed.Select(action => new XElement("Massnahme", action))
                ),
                !string.IsNullOrEmpty(emergency.Outcome) ? new XElement("Ausgang", emergency.Outcome) : null
            ))
        );
    }

    private XElement GenerateStatistics(IEnumerable<Session> sessions, IEnumerable<Client> clients, IEnumerable<EmergencyEvent> emergencies)
    {
        var sessionsList = sessions.ToList();
        var clientsList = clients.ToList();
        var emergenciesList = emergencies.ToList();

        return new XElement("Statistiken",
            new XElement("Gesamtzahlen",
                new XElement("AnzahlKlienten", clientsList.Count),
                new XElement("AnzahlKonsumvorgaenge", sessionsList.Count),
                new XElement("AnzahlNotfaelle", emergenciesList.Count),
                new XElement("DurchschnittlicheDauer", sessionsList.Any() ? sessionsList.Average(s => s.Duration.TotalMinutes).ToString("F1") : "0")
            ),
            new XElement("SubstanzVerteilung",
                sessionsList.GroupBy(s => s.Substance)
                    .Select(g => new XElement("Substanz",
                        new XAttribute("name", GetKDSSubstance(g.Key)),
                        new XAttribute("anzahl", g.Count()),
                        new XAttribute("anteil", (g.Count() * 100.0 / sessionsList.Count).ToString("F1") + "%")
                    ))
            ),
            new XElement("GeschlechterVerteilung",
                clientsList.GroupBy(c => c.Gender)
                    .Select(g => new XElement("Geschlecht",
                        new XAttribute("name", GetKDSGender(g.Key)),
                        new XAttribute("anzahl", g.Count()),
                        new XAttribute("anteil", (g.Count() * 100.0 / clientsList.Count).ToString("F1") + "%")
                    ))
            ),
            new XElement("AltersVerteilung",
                clientsList.GroupBy(c => GetAgeGroup(DateTime.Now.Year - c.BirthYear))
                    .Select(g => new XElement("Altersgruppe",
                        new XAttribute("name", g.Key),
                        new XAttribute("anzahl", g.Count())
                    ))
            )
        );
    }

    private async Task<List<Client>> GetUniqueClientsFromSessions(IEnumerable<Session> sessions)
    {
        var clientIds = sessions.Select(s => s.ClientId).Distinct();
        var clients = new List<Client>();
        
        foreach (var clientId in clientIds)
        {
            var client = await _clientRepository.GetByIdAsync(clientId);
            if (client != null)
            {
                clients.Add(client);
            }
        }
        
        return clients;
    }

    private KDSValidationResult ValidateKDSSchema(string xmlContent)
    {
        try
        {
            var doc = XDocument.Parse(xmlContent);
            
            // Basis-Validierung
            var errors = new List<string>();
            
            if (doc.Root?.Attribute("version")?.Value != "3.0")
            {
                errors.Add("Falsche KDS-Version");
            }
            
            if (!doc.Descendants("Einrichtung").Any())
            {
                errors.Add("Einrichtungsdaten fehlen");
            }
            
            if (!doc.Descendants("Klienten").Any())
            {
                errors.Add("Klientendaten fehlen");
            }

            return new KDSValidationResult
            {
                IsValid = !errors.Any(),
                Errors = errors,
                Warnings = new List<string>() // TODO: Implementiere Warnungen
            };
        }
        catch (Exception ex)
        {
            return new KDSValidationResult
            {
                IsValid = false,
                Errors = new List<string> { $"XML-Parsing Fehler: {ex.Message}" }
            };
        }
    }

    // Mapping-Funktionen für KDS-Standards
    private string GetKDSGender(Gender gender) => gender switch
    {
        Gender.Male => "M",
        Gender.Female => "W",
        Gender.Diverse => "D",
        _ => "U"
    };

    private string GetKDSSubstance(SubstanceType substance) => substance switch
    {
        SubstanceType.Heroin => "HEROIN",
        SubstanceType.Cocaine => "KOKAIN",
        SubstanceType.Amphetamines => "AMPHETAMINE",
        SubstanceType.OtherOpioids => "OPIOIDE_ANDERE",
        SubstanceType.Cannabis => "CANNABIS",
        _ => "ANDERE"
    };

    private string GetKDSApplicationMethod(ApplicationMethod method) => method switch
    {
        ApplicationMethod.Intravenous => "INTRAVEN_S",
        ApplicationMethod.Inhalation => "INHALATIV",
        ApplicationMethod.Intranasal => "INTRANASAL",
        ApplicationMethod.Oral => "ORAL",
        _ => "ANDERE"
    };

    private string GetKDSSessionStatus(SessionStatus status) => status switch
    {
        SessionStatus.Completed => "BEENDET",
        SessionStatus.Emergency => "NOTFALL",
        SessionStatus.Active => "AKTIV",
        _ => "ANDERE"
    };

    private string GetKDSEmergencyType(Entities.EmergencyType type) => type switch
    {
        Entities.EmergencyType.Overdose => "UEBERDOSIS",
        Entities.EmergencyType.Unconsciousness => "BEWUSSTLOSIGKEIT",
        Entities.EmergencyType.Seizure => "KRAMPFANFALL",
        Entities.EmergencyType.RespiratoryArrest => "ATEMSTILLSTAND",
        Entities.EmergencyType.Injury => "VERLETZUNG",
        Entities.EmergencyType.Cardiac => "HERZSTILLSTAND",
        Entities.EmergencyType.Psychiatric => "PSYCHISCHER_NOTFALL",
        Entities.EmergencyType.PsychiatricEmergency => "PSYCHISCHER_NOTFALL",
        _ => "ANDERE"
    };

    private string GetKDSTreatmentHistory(TreatmentHistory history) => history switch
    {
        TreatmentHistory.None => "KEINE",
        TreatmentHistory.Detoxification => "ENTGIFTUNG",
        TreatmentHistory.Substitution => "SUBSTITUTION",
        TreatmentHistory.Rehabilitation => "ENTWOEHNUNG",
        TreatmentHistory.Aftercare => "NACHSORGE",
        _ => "UNBEKANNT"
    };

    private string GetAgeGroup(int age) => age switch
    {
        < 18 => "Unter 18",
        >= 18 and < 25 => "18-24",
        >= 25 and < 35 => "25-34",
        >= 35 and < 45 => "35-44",
        >= 45 and < 55 => "45-54",
        >= 55 => "55+"
    };
}

public class KDSExportResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string FileName { get; set; } = string.Empty;
    public byte[]? FileContent { get; set; }
    public string XmlContent { get; set; } = string.Empty;
    public KDSValidationResult ValidationResult { get; set; } = new();
    public int SessionCount { get; set; }
    public int ClientCount { get; set; }
    public int EmergencyCount { get; set; }
    public DateTime ExportedAt { get; set; }
}

public class KDSValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}