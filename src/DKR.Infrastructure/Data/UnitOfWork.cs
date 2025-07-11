using DKR.Core.Interfaces;
using DKR.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace DKR.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly DKRDbContext _context;
    private IDbContextTransaction? _transaction;

    private IClientRepository? _clients;
    private ISessionRepository? _sessions;
    private IAuditRepository? _auditLogs;
    private IEmergencyRepository? _emergencyEvents;
    private IHarmReductionRepository? _harmReduction;

    public UnitOfWork(DKRDbContext context)
    {
        _context = context;
    }

    public IClientRepository Clients => _clients ??= new ClientRepository(_context);
    public ISessionRepository Sessions => _sessions ??= new SessionRepository(_context);
    public IAuditRepository AuditLogs => _auditLogs ??= new AuditRepository(_context);
    public IEmergencyRepository EmergencyEvents => _emergencyEvents ??= new EmergencyRepository(_context);
    public IHarmReductionRepository HarmReduction => _harmReduction ??= new HarmReductionRepository(_context);

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            await _transaction!.CommitAsync();
        }
        finally
        {
            await _transaction!.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        try
        {
            await _transaction!.RollbackAsync();
        }
        finally
        {
            await _transaction!.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}