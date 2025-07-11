using DKR.Core.Entities;
using DKR.Core.Interfaces;
using DKR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DKR.Infrastructure.Repositories
{
    public class EmergencyRepository : IEmergencyRepository
    {
        // Inject your DbContext or data source here
        private readonly DKRDbContext _context;

        public EmergencyRepository(DKRDbContext context)
        {
            _context = context;
        }

        public async Task<EmergencyEvent> CreateAsync(EmergencyEvent emergencyEvent)
        {
            _context.EmergencyEvents.Add(emergencyEvent);
            await _context.SaveChangesAsync();
            return emergencyEvent;
        }

        public async Task<EmergencyEvent> GetByIdAsync(string id)
        {
            var emergencyEvent = await _context.EmergencyEvents.FindAsync(id);
            return emergencyEvent ?? throw new ArgumentException($"EmergencyEvent with ID {id} not found", nameof(id));
        }


        public async Task<EmergencyEvent> UpdateAsync(EmergencyEvent emergencyEvent)
        {
            _context.EmergencyEvents.Update(emergencyEvent);
            await _context.SaveChangesAsync();
            return emergencyEvent;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var emergencyEvent = await _context.EmergencyEvents.FindAsync(id);
                if (emergencyEvent != null)
                {
                    _context.EmergencyEvents.Remove(emergencyEvent);
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

        public async Task<IEnumerable<EmergencyEvent>> GetByDateRangeAsync(DateTime from, DateTime to)
        {
            return await _context.EmergencyEvents
                .Include(s => s.Client)
                .Where(s => s.OccurredAt >= from && s.OccurredAt <= to)
                .OrderByDescending(s => s.OccurredAt)
                .ToListAsync();
        }
       

        public async Task<IEnumerable<EmergencyEvent>> GetByClientIdAsync(string clientId)
        {
            return await _context.EmergencyEvents
                .Where(x => x.ClientId == clientId)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmergencyEvent>> GetAllEmergencyEventsAsync()
        {
            return await _context.EmergencyEvents
                .Include(s=>s.Session)
             .OrderByDescending(c => c.OccurredAt)
             .ToListAsync();
        }
    }
}