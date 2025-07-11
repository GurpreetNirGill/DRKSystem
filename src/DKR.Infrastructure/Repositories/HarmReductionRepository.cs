using DKR.Core.Entities;
using DKR.Core.Interfaces;
using DKR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DKR.Infrastructure.Repositories
{
    public class HarmReductionRepository : IHarmReductionRepository
    {
         private readonly DKRDbContext _context;

        public HarmReductionRepository(DKRDbContext context)
        {
            _context = context;
        }

        public async Task<HarmReduction> CreateAsync(HarmReduction harmReduction)
        {
            _context.HarmReductions.Add(harmReduction);
            await _context.SaveChangesAsync();
            return harmReduction;
        }

        public async Task<HarmReduction> GetByIdAsync(string id)
        {
            var harmReduction = await _context.HarmReductions.FindAsync(id);
            return harmReduction ?? throw new ArgumentException($"HarmReduction with ID {id} not found", nameof(id));
        }
        

        public async Task<HarmReduction> UpdateAsync(HarmReduction harmReduction)
        {
            _context.HarmReductions.Update(harmReduction);
            await _context.SaveChangesAsync();
            return harmReduction;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var harmReduction = await _context.HarmReductions.FindAsync(id);
                if (harmReduction != null)
                {
                    _context.HarmReductions.Remove(harmReduction);
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
        public async Task<IEnumerable<HarmReduction>> GetServicesByDateRangeAsync(DateTime from, DateTime to)
        {
            return await _context.HarmReductions
                .Include(s => s.Client)
                .Where(s => s.ScheduledAt >= from && s.CompletedAt <= to)
                .OrderByDescending(s => s.ScheduledAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<HarmReduction>> GetByTypeAsync(ServiceType serviceType)
        {
            return await _context.HarmReductions
                .Where(x => x.Type == serviceType)
                .ToListAsync();
        }

        public async Task<IEnumerable<HarmReduction>> GetByClientIdAsync(string clientId)
        {
            return await _context.HarmReductions
                .Where(x => x.ClientId == clientId)
                .ToListAsync();
        }
        public async Task<IEnumerable<HarmReduction>> GetAllAsync()
        {
            return await _context.HarmReductions
              .OrderByDescending(c => c.CreatedAt)
              .ToListAsync();
        }
    }
}