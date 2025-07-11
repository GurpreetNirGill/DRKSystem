using DKR.Core.Entities;
using DKR.Core.Interfaces;
using DKR.Shared.Enums;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace DKR.Core.Services;

public class TDIExportService
{
    private readonly IClientRepository _clientRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IHarmReductionRepository _harmReductionRepository;
    private readonly IAuditService _auditService;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public TDIExportService(
        IClientRepository clientRepository,
        ISessionRepository sessionRepository,
        IHarmReductionRepository harmReductionRepository,
        IAuditService auditService,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _clientRepository = clientRepository;
        _sessionRepository = sessionRepository;
        _harmReductionRepository = harmReductionRepository;
        _auditService = auditService;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<TDIExportResult> ExportTDI30ToEMCDDAAsync(DateTime from, DateTime to)
    {
        try
        {
            // 1. Daten für TDI 3.0 sammeln
            var tdiData = await CollectTDIDataAsync(from, to);

            // 2. TDI 3.0 JSON Format erstellen
            var tdiJson = GenerateTDI30JSON(tdiData, from, to);

            // 3. Validierung
            var validationResult = ValidateTDIData(tdiData);

            if (!validationResult.IsValid)
            {
                return new TDIExportResult
                {
                    Success = false,
                    ErrorMessage = string.Join("; ", validationResult.Errors)
                };
            }

            // 4. Upload zu REITOX Focal Point (Deutschland - IFT München)
            var uploadResult = await UploadToREITOXAsync(tdiJson);

            // 5. Audit-Log
            await _auditService.LogAsync("TDIExportCompleted", "Export", $"TDI30_{from:yyyyMM}", 
                $"TDI 3.0 Export an EMCDDA: {tdiData.TreatmentDemands.Count} Datensätze");

            return new TDIExportResult
            {
                Success = uploadResult.Success,
                FileName = $"TDI30_DE_HH_{from:yyyyMM}.json",
                JsonContent = tdiJson,
                ValidationResult = validationResult,
                RecordCount = tdiData.TreatmentDemands.Count,
                UploadResult = uploadResult,
                ExportedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            await _auditService.LogAsync("TDIExportFailed", "Export", $"{from:yyyyMM}", 
                $"TDI Export fehlgeschlagen: {ex.Message}");
            
            return new TDIExportResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task<TDI30Data> CollectTDIDataAsync(DateTime from, DateTime to)
    {
        var clients = await _clientRepository.GetAllAsync(1, 10000); // TODO: Pagination
        var sessions = await _sessionRepository.GetSessionsByDateRangeAsync(from, to);
        var harmReductionServices = await _harmReductionRepository.GetServicesByDateRangeAsync(from, to);

        var treatmentDemands = clients.Where(c => c.FirstVisitDate >= from && c.FirstVisitDate <= to)
            .Select(client => MapClientToTDI(client, sessions.Where(s => s.ClientId == client.Id), (IEnumerable<Entities.HarmReduction>)harmReductionServices.Where(h => h.ClientId == client.Id)))
            .ToList();

        return new TDI30Data
        {
            Country = "DE",
            Region = "HH", // Hamburg
            FacilityID = "DE-HH-DKR-001",
            ReportingPeriod = new ReportingPeriod
            {
                From = from,
                To = to,
                Year = from.Year,
                Month = from.Month
            },
            TreatmentDemands = treatmentDemands
        };
    }

    private TDITreatmentDemand MapClientToTDI(Client client, IEnumerable<Session> sessions, IEnumerable<Entities.HarmReduction> services)
    {
        var sessionsList = sessions.ToList();
        var servicesList = services.ToList();

        return new TDITreatmentDemand
        {
            // Anonymisierte ID (EMCDDA-konform)
            TreatmentDemandID = GenerateAnonymousID(client.UUID),
            
            // Demografische Daten
            Gender = MapGenderToTDI(client.Gender),
            Age = DateTime.Now.Year - client.BirthYear,
            CountryOfBirth = "DE", // Annahme
            Region = ExtractRegionFromPostalCode(client.PostalCode),
            
            // Substanz-Informationen
            PrimaryDrug = MapSubstanceToTDI(client.MainSubstance),
            PrimaryRouteOfAdministration = GetPrimaryRoute(sessionsList),
            SecondaryDrugs = GetSecondaryDrugs(sessionsList),
            
            // Behandlungshistorie
            PreviousTreatment = MapTreatmentHistoryToTDI(client.TreatmentHistory),
            SourceOfReferral = client.FirstVisit ? "SELF" : "OTHER",
            
            // Aktuelle Behandlung
            TreatmentType = "HARM_REDUCTION",
            TreatmentSetting = "OUTPATIENT",
            TreatmentStartDate = client.FirstVisitDate,
            
            // Harm Reduction spezifische Daten
            HarmReductionServices = MapHarmReductionServices(servicesList),
            NumberOfContacts = sessionsList.Count,
            
            // Sozioökonomische Daten (optional/anonymisiert)
            EducationLevel = "UNKNOWN", // Nicht erfasst in DKR
            EmploymentStatus = "UNKNOWN", // Nicht erfasst in DKR
            LivingSituation = "UNKNOWN", // Nicht erfasst in DKR
            
            // Risikoverhalten
            InjectionBehavior = sessionsList.Any(s => s.ApplicationMethod == ApplicationMethod.Intravenous),
            HIVStatus = GetHIVStatusFromServices(servicesList),
            HCVStatus = GetHCVStatusFromServices(servicesList)
        };
    }

    private string GenerateTDI30JSON(TDI30Data tdiData, DateTime from, DateTime to)
    {
        var export = new
        {
            metadata = new
            {
                version = "3.0",
                country = tdiData.Country,
                region = tdiData.Region,
                facilityID = tdiData.FacilityID,
                reportingPeriod = new
                {
                    from = from.ToString("yyyy-MM-dd"),
                    to = to.ToString("yyyy-MM-dd"),
                    year = from.Year,
                    month = from.Month
                },
                exportDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                contact = new
                {
                    organization = "DKR Hamburg-Altona",
                    email = "data@dkr-hamburg.de",
                    phone = "+49 40 123456789"
                }
            },
            data = new
            {
                treatmentDemands = tdiData.TreatmentDemands.Select(td => new
                {
                    treatmentDemandID = td.TreatmentDemandID,
                    demographics = new
                    {
                        gender = td.Gender,
                        age = td.Age,
                        countryOfBirth = td.CountryOfBirth,
                        region = td.Region
                    },
                    substances = new
                    {
                        primaryDrug = td.PrimaryDrug,
                        primaryRouteOfAdministration = td.PrimaryRouteOfAdministration,
                        secondaryDrugs = td.SecondaryDrugs
                    },
                    treatment = new
                    {
                        treatmentType = td.TreatmentType,
                        treatmentSetting = td.TreatmentSetting,
                        treatmentStartDate = td.TreatmentStartDate.ToString("yyyy-MM-dd"),
                        previousTreatment = td.PreviousTreatment,
                        sourceOfReferral = td.SourceOfReferral
                    },
                    harmReduction = new
                    {
                        services = td.HarmReductionServices,
                        numberOfContacts = td.NumberOfContacts,
                        injectionBehavior = td.InjectionBehavior
                    },
                    healthStatus = new
                    {
                        hivStatus = td.HIVStatus,
                        hcvStatus = td.HCVStatus
                    },
                    socioeconomic = new
                    {
                        educationLevel = td.EducationLevel,
                        employmentStatus = td.EmploymentStatus,
                        livingSituation = td.LivingSituation
                    }
                }).ToArray(),
                summary = new
                {
                    totalRecords = tdiData.TreatmentDemands.Count,
                    demographics = CalculateDemographics(tdiData.TreatmentDemands),
                    substances = CalculateSubstanceStatistics(tdiData.TreatmentDemands),
                    treatments = CalculateTreatmentStatistics(tdiData.TreatmentDemands)
                }
            }
        };

        return JsonSerializer.Serialize(export, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private async Task<EMCDDAUploadResult> UploadToREITOXAsync(string jsonContent)
    {
        try
        {
            var reitoxEndpoint = _configuration["EMCDDA:REITOXEndpoint"] ?? "https://api.reitox.emcdda.europa.eu/tdi/v3/upload";
            var apiKey = _configuration["EMCDDA:APIKey"] ?? "";

            if (string.IsNullOrEmpty(apiKey))
            {
                return new EMCDDAUploadResult
                {
                    Success = false,
                    ErrorMessage = "REITOX API-Key nicht konfiguriert"
                };
            }

            var request = new HttpRequestMessage(HttpMethod.Post, reitoxEndpoint);
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Headers.Add("X-Country-Code", "DE");
            request.Headers.Add("X-Facility-ID", "DE-HH-DKR-001");
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var uploadResponse = JsonSerializer.Deserialize<REITOXUploadResponse>(responseContent);
                
                return new EMCDDAUploadResult
                {
                    Success = true,
                    UploadID = uploadResponse?.UploadID ?? "",
                    ValidationReport = uploadResponse?.ValidationReport ?? "",
                    ProcessedRecords = uploadResponse?.ProcessedRecords ?? 0
                };
            }
            else
            {
                return new EMCDDAUploadResult
                {
                    Success = false,
                    ErrorMessage = $"Upload fehlgeschlagen: {response.StatusCode} - {responseContent}"
                };
            }
        }
        catch (Exception ex)
        {
            return new EMCDDAUploadResult
            {
                Success = false,
                ErrorMessage = $"Upload-Fehler: {ex.Message}"
            };
        }
    }

    // Mapping-Funktionen für TDI 3.0 Standards
    private string MapGenderToTDI(Gender gender) => gender switch
    {
        Gender.Male => "MALE",
        Gender.Female => "FEMALE",
        Gender.Diverse => "OTHER",
        _ => "UNKNOWN"
    };

    private string MapSubstanceToTDI(SubstanceType substance) => substance switch
    {
        SubstanceType.Heroin => "HEROIN",
        SubstanceType.Cocaine => "COCAINE",
        SubstanceType.Amphetamines => "AMPHETAMINES",
        SubstanceType.OtherOpioids => "OTHER_OPIOIDS",
        SubstanceType.Cannabis => "CANNABIS",
        _ => "OTHER"
    };

    private string MapTreatmentHistoryToTDI(TreatmentHistory history) => history switch
    {
        TreatmentHistory.None => "NEVER",
        TreatmentHistory.Detoxification => "DETOXIFICATION",
        TreatmentHistory.Substitution => "SUBSTITUTION",
        TreatmentHistory.Rehabilitation => "REHABILITATION",
        TreatmentHistory.Aftercare => "AFTERCARE",
        _ => "UNKNOWN"
    };

    private string GenerateAnonymousID(string uuid)
    {
        // Generiere anonyme ID aus UUID für EMCDDA (Hash-basiert)
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(uuid + "EMCDDA_SALT"));
            return Convert.ToHexString(hash)[..16]; // Erste 16 Zeichen
        }
    }

    private string GetPrimaryRoute(List<Session> sessions)
    {
        if (!sessions.Any()) return "UNKNOWN";
        
        var mostCommon = sessions.GroupBy(s => s.ApplicationMethod)
            .OrderByDescending(g => g.Count())
            .First().Key;
            
        return mostCommon switch
        {
            ApplicationMethod.Intravenous => "INJECT",
            ApplicationMethod.Inhalation => "SMOKE",
            ApplicationMethod.Intranasal => "SNORT",
            ApplicationMethod.Oral => "ORAL",
            _ => "OTHER"
        };
    }

    private List<string> GetSecondaryDrugs(List<Session> sessions)
    {
        return sessions.GroupBy(s => s.Substance)
            .Skip(1) // Skip primary
            .Select(g => MapSubstanceToTDI(g.Key))
            .ToList();
    }

    private TDIValidationResult ValidateTDIData(TDI30Data tdiData)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Validierung der Pflichtfelder
        foreach (var demand in tdiData.TreatmentDemands)
        {
            if (string.IsNullOrEmpty(demand.TreatmentDemandID))
                errors.Add($"TreatmentDemandID fehlt");
                
            if (string.IsNullOrEmpty(demand.Gender))
                errors.Add($"Gender fehlt für {demand.TreatmentDemandID}");
                
            if (demand.Age < 10 || demand.Age > 100)
                warnings.Add($"Ungewöhnliches Alter für {demand.TreatmentDemandID}: {demand.Age}");
        }

        return new TDIValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors,
            Warnings = warnings
        };
    }

    // Helper Methoden
    private List<string> MapHarmReductionServices(List<Entities.HarmReduction> services) => 
        services.Select(s => s.Type.ToString().ToUpper()).Distinct().ToList();

    private string GetHIVStatusFromServices(List<Entities.HarmReduction> services) =>
        services.Any(s => s.Type == Entities.ServiceType.HIVTest) ? "TESTED" : "UNKNOWN";

    private string GetHCVStatusFromServices(List<Entities.HarmReduction> services) =>
        services.Any(s => s.Type == Entities.ServiceType.HCVTest) ? "TESTED" : "UNKNOWN";

    private string ExtractRegionFromPostalCode(string? postalCode) =>
        postalCode?.Substring(0, 2) ?? "UNKNOWN";

    private object CalculateDemographics(List<TDITreatmentDemand> demands) => new
    {
        totalMale = demands.Count(d => d.Gender == "MALE"),
        totalFemale = demands.Count(d => d.Gender == "FEMALE"),
        averageAge = demands.Average(d => d.Age)
    };

    private object CalculateSubstanceStatistics(List<TDITreatmentDemand> demands) => 
        demands.GroupBy(d => d.PrimaryDrug)
            .ToDictionary(g => g.Key, g => g.Count());

    private object CalculateTreatmentStatistics(List<TDITreatmentDemand> demands) => new
    {
        totalHarmReduction = demands.Count(d => d.TreatmentType == "HARM_REDUCTION"),
        totalContacts = demands.Sum(d => d.NumberOfContacts),
        injectionUsers = demands.Count(d => d.InjectionBehavior)
    };
}

// TDI 3.0 Datenmodelle
public class TDI30Data
{
    public string Country { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string FacilityID { get; set; } = string.Empty;
    public ReportingPeriod ReportingPeriod { get; set; } = new();
    public List<TDITreatmentDemand> TreatmentDemands { get; set; } = new();
}

public class ReportingPeriod
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
}

public class TDITreatmentDemand
{
    public string TreatmentDemandID { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public int Age { get; set; }
    public string CountryOfBirth { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string PrimaryDrug { get; set; } = string.Empty;
    public string PrimaryRouteOfAdministration { get; set; } = string.Empty;
    public List<string> SecondaryDrugs { get; set; } = new();
    public string PreviousTreatment { get; set; } = string.Empty;
    public string SourceOfReferral { get; set; } = string.Empty;
    public string TreatmentType { get; set; } = string.Empty;
    public string TreatmentSetting { get; set; } = string.Empty;
    public DateTime TreatmentStartDate { get; set; }
    public List<string> HarmReductionServices { get; set; } = new();
    public int NumberOfContacts { get; set; }
    public string EducationLevel { get; set; } = string.Empty;
    public string EmploymentStatus { get; set; } = string.Empty;
    public string LivingSituation { get; set; } = string.Empty;
    public bool InjectionBehavior { get; set; }
    public string HIVStatus { get; set; } = string.Empty;
    public string HCVStatus { get; set; } = string.Empty;
}

public class TDIExportResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string JsonContent { get; set; } = string.Empty;
    public TDIValidationResult ValidationResult { get; set; } = new();
    public int RecordCount { get; set; }
    public EMCDDAUploadResult UploadResult { get; set; } = new();
    public DateTime ExportedAt { get; set; }
}

public class TDIValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class EMCDDAUploadResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string UploadID { get; set; } = string.Empty;
    public string ValidationReport { get; set; } = string.Empty;
    public int ProcessedRecords { get; set; }
}

public class REITOXUploadResponse
{
    public string UploadID { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ValidationReport { get; set; } = string.Empty;
    public int ProcessedRecords { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}