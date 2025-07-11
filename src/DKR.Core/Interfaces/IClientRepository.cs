using DKR.Core.Entities;

namespace DKR.Core.Interfaces;

public interface IClientRepository
{
    Task<Client> GetByIdAsync(string clientId);
    Task<Client> GetByUuidAsync(string uuid);
    Task<IEnumerable<Client>> GetAllAsync(int? pageNumber = null, int? pageSize = null);
    Task<Client> CreateAsync(Client client);
    Task<Client> UpdateAsync(Client client);
    Task<bool> DeleteAsync(string clientId);
    Task<bool> ExistsByUuidAsync(string uuid);
    Task<IEnumerable<Client>> SearchAsync(string searchTerm);
    Task<string> GenerateNextUuidAsync();
}