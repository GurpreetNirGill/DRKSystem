using DKR.Core.Entities;
using DKR.Core.Interfaces;
using DKR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace DKR.Infrastructure.Repositories
{
    public class AuditRepository : IAuditRepository
    {
        // Inject your DbContext or data source here
        private readonly DKRDbContext _context;

        public AuditRepository(DKRDbContext context)
        {
            _context = context;
        }

        public async Task<AuditLog> CreateAsync(AuditLog auditLog)
        {
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
            return auditLog;
        }

        public async Task<AuditLog> GetByIdAsync(string id)
        {
            var auditLog = await _context.AuditLogs.FindAsync(id);
            return auditLog ?? throw new ArgumentException($"AuditLog with ID {id} not found", nameof(id));
        }


        public async Task<AuditLog> UpdateAsync(AuditLog auditLog)
        {
            _context.AuditLogs.Update(auditLog);
            await _context.SaveChangesAsync();
            return auditLog;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var auditLog = await _context.AuditLogs.FindAsync(id);
                if (auditLog != null)
                {
                    _context.AuditLogs.Remove(auditLog);
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

        public async Task<AuditLog?> GetLastAuditLogAsync()
        {
            return await _context.AuditLogs
          .OrderByDescending(s => s.Timestamp)
          .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync()
        {
            return await _context.AuditLogs
             .OrderByDescending(c => c.Timestamp)
             .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsByDateRangeAsync(DateTime from, DateTime to)
        {
            return await _context.AuditLogs
                 .Where(s => s.Timestamp >= from && s.Timestamp <= to)
                 .OrderByDescending(s => s.Timestamp)
                 .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsByEntityAsync(string entityType, string entityId)
        {
            return await _context.AuditLogs
                .Where(s => s.EntityId == entityId && s.EntityType== entityType)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsByUserAsync(string userId)
        {
            return await _context.AuditLogs
                 .Where(s => s.UserId == userId)
                 .ToListAsync();
        }
    }
}