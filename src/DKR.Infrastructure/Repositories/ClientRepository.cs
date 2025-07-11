using DKR.Core.Entities;
using DKR.Core.Interfaces;
using DKR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DKR.Infrastructure.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly DKRDbContext _context;

    public ClientRepository(DKRDbContext context)
    {
        _context = context;
    }

    public async Task<Client> CreateAsync(Client client)
    {
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        return client;
    }

    public async Task<Client> GetByIdAsync(string id)
    {
        var client = await _context.Clients.FindAsync(id);
        return client ?? throw new ArgumentException($"Client with ID {id} not found", nameof(id));
    }

    public async Task<Client?> GetByUuidAsync(string uuid)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.UUID == uuid);

        return client;
    }

    public async Task<bool> ExistsByUuidAsync(string uuid)
    {
        return await _context.Clients
            .AnyAsync(c => c.UUID == uuid);
    }

    public async Task<string> GenerateNextUuidAsync()
    {
        var currentYear = DateTime.UtcNow.Year;
        var yearPrefix = $"KL-{currentYear}-";

        var lastClient = await _context.Clients
            .Where(c => c.UUID.StartsWith(yearPrefix))
            .OrderByDescending(c => c.UUID)
            .FirstOrDefaultAsync();

        int nextSequence = 1;
        if (lastClient != null)
        {
            // Extract sequence number from UUID format KL-YYYY-NNNN
            var parts = lastClient.UUID.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int lastSequence))
            {
                nextSequence = lastSequence + 1;
            }
        }

        return $"KL-{currentYear}-{nextSequence:D4}";
    }

    public async Task<IEnumerable<Client>?> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return null;
        }

        var term = searchTerm.Trim();

        var clients = await _context.Clients
            .Where(c =>
                (c.UUID != null && c.UUID.Contains(term)) ||
                (c.PostalCode != null && c.PostalCode.Contains(term)) ||
                (c.Nationality != null && c.Nationality.Contains(term)))
            .ToListAsync();

        return clients.Any() ? clients.OrderByDescending(c => c.LastCheckIn)
            .Take(20) : null;
    }

    public async Task<IEnumerable<Client>> FindByAttributesAsync(int age, string gender, string postalCode)
    {
        var query = _context.Clients.AsQueryable();

        // Age range matching (±2 years)
        query = query.Where(c => Math.Abs(c.Age - age) <= 2);

        // Gender matching
        if (!string.IsNullOrEmpty(gender))
        {
            query = query.Where(c => c.Gender.ToString() == gender);
        }

        // Postal code matching (first 3 digits)
        if (!string.IsNullOrEmpty(postalCode) && postalCode.Length >= 3)
        {
            var postalPrefix = postalCode.Substring(0, 3);
            query = query.Where(c => c.PostalCode.StartsWith(postalPrefix));
        }

        return await query
            .OrderByDescending(c => c.LastCheckIn)
            .Take(10)
            .ToListAsync();
    }

   

    public async Task<IEnumerable<Client>> GetAllAsync(int? pageNumber = null, int? pageSize = null)
    {
        var query = _context.Clients.OrderByDescending(c => c.CreatedAt).AsQueryable();

        if (pageNumber.HasValue && pageSize.HasValue)
        {
            query = query
                .Skip((pageNumber.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<Client> UpdateAsync(Client client)
    {
        _context.Clients.Update(client);
        await _context.SaveChangesAsync();
        return client;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IEnumerable<Client>> GetActiveClientsAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Clients
            .Where(c => c.LastCheckIn.HasValue && c.LastCheckIn.Value.Date == today)
            .OrderByDescending(c => c.LastCheckIn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Client>> GetClientsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Clients
            .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }
}