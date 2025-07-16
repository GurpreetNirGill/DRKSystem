using DKR.Core.Entities;
using DKR.Core.Interfaces;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Linq;

namespace DKR.Core.Services;

public class ClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly INotificationService _notificationService;
    private readonly IHttpContextAccessor httpContextAccessor;
    public ClientService(IClientRepository clientRepository, INotificationService notificationService, IHttpContextAccessor httpContextAccessor)
    {
        _clientRepository = clientRepository;
        _notificationService = notificationService;
        this.httpContextAccessor = httpContextAccessor;

    }

    public async Task<Client> CreateClientAsync(Client client)
    {
        // UUID generieren
        client.UUID = await GenerateUniqueUuidAsync();
        client.CreatedAt = DateTime.UtcNow;
        client.FirstVisitDate = DateTime.UtcNow;
        client.LastVisitDate = DateTime.UtcNow;

        // Duplikatcheck
        var duplicateCheck = await CheckForDuplicatesAsync(client);
        if (duplicateCheck.HasPotentialDuplicate)
        {
            await _notificationService.NotifyAsync("Warnung",
                $"Mögliche Dublette gefunden: {duplicateCheck.Message}",
                NotificationType.Warning);
        }

        var createdClient = await _clientRepository.CreateAsync(client);

        await _notificationService.NotifyAsync("Erfolg",
            $"Neuer Klient erstellt: {createdClient.UUID}",
            NotificationType.Success);

        return createdClient;
    }

    public async Task<Client?> CheckInAsync(string identifier)
    {
        Client? client = null;

        // Versuche UUID-Suche
        if (IsValidUuid(identifier))
        {
            client = await _clientRepository.GetByUuidAsync(identifier);
        }

        // Fallback: Allgemeine Suche
        if (client == null)
        {
            var searchResults = await _clientRepository.SearchAsync(identifier);
            if (searchResults != null)
                client = searchResults?.FirstOrDefault();
        }

        if (client != null)
        {
            // Update letzter Besuch
            client.LastVisitDate = DateTime.UtcNow;
            await _clientRepository.UpdateAsync(client);

            await _notificationService.NotifyAsync("Check-in",
                $"Klient {client.UUID} eingecheckt",
                NotificationType.Info);
        }

        return client;
    }

    public async Task<string> GenerateUniqueUuidAsync()
    {
        var year = DateTime.Now.Year;
        var baseUuid = $"KL-{year}-";

        // Suche nächste verfügbare Nummer
        for (int i = 1; i <= 9999; i++)
        {
            var uuid = baseUuid + i.ToString("D4");
            if (!await _clientRepository.ExistsByUuidAsync(uuid))
            {
                return uuid;
            }
        }

        throw new InvalidOperationException("Keine verfügbaren UUIDs für das Jahr " + year);
    }

    private async Task<DuplicateCheckResult> CheckForDuplicatesAsync(Client client)
    {
        // Einfacher Duplikatcheck basierend auf Geschlecht und Geburtsjahr
        var existingClients = await _clientRepository.GetAllAsync(); // TODO: Verbessern

        var potentialDuplicates = existingClients.Where(c =>
            c.Gender == client.Gender &&
            c.BirthYear == client.BirthYear &&
            c.PostalCode == client.PostalCode).ToList();

        if (potentialDuplicates.Any())
        {
            return new DuplicateCheckResult
            {
                HasPotentialDuplicate = true,
                Message = $"Gefunden: {potentialDuplicates.Count} Klient(en) mit gleichem Profil"
            };
        }

        return new DuplicateCheckResult { HasPotentialDuplicate = false };
    }

    private bool IsValidUuid(string identifier)
    {
        return identifier.StartsWith("KL-") && identifier.Length == 12;
    }

    public async Task<List<string>> GetAllClientIdAsync()
    {
        var clients = await _clientRepository.GetAllAsync();
        var clientIds = clients.Select(x => x.Id).ToList();
        return clientIds;
    }
    public async Task<Client> GetByIdAsync(string clientId)
    {
        var client = await _clientRepository.GetByIdAsync(clientId);
        return client;
    }
    public async Task<List<Client>> GetAllClientAsync()
    {
        var clients = await _clientRepository.GetAllAsync();
        return clients.ToList();
    }
}

public class DuplicateCheckResult
{
    public bool HasPotentialDuplicate { get; set; }
    public string Message { get; set; } = string.Empty;
}