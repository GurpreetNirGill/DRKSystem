namespace DKR.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IClientRepository Clients { get; }
    ISessionRepository Sessions { get; }
    IAuditRepository AuditLogs { get; }
    IEmergencyRepository EmergencyEvents { get; }
    IHarmReductionRepository HarmReduction { get; }
    
    Task<int> CompleteAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}