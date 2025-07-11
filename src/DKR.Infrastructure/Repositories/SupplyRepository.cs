using DKR.Core.Entities;
using DKR.Core.Interfaces;
using DKR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DKR.Infrastructure.Repositories
{
    public class SupplyRepository : ISupplyRepository
    {
        // Inject your DbContext or data source here
        private readonly DKRDbContext _context;

        public SupplyRepository(DKRDbContext context)
        {
            _context = context;
        }

        public async Task<Supply> CreateAsync(Supply supply)
        {
            _context.Supply.Add(supply);
            await _context.SaveChangesAsync();
            return supply;
        }

        public async Task<Supply> GetByIdAsync(string id)
        {
            var supply = await _context.Supply.FindAsync(id);
            return supply ?? throw new ArgumentException($"Supply with ID {id} not found", nameof(id));
        }


        public async Task<Supply> UpdateAsync(Supply supply)
        {
            _context.Supply.Update(supply);
            await _context.SaveChangesAsync();
            return supply;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var supply = await _context.Supply.FindAsync(id);
                if (supply != null)
                {
                    _context.Supply.Remove(supply);
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
        public async Task<IEnumerable<Supply>> GetBySessionIdAsync(string sessionId)
        {
            return await _context.Supply
                .Where(x => x.SessionId == sessionId)
                .ToListAsync();
        }
    }
}