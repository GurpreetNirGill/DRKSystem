using DKR.Core.Entities;
using DKR.Core.Interfaces;
using DKR.Infrastructure.Data;
using DKR.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DKR.Infrastructure.Repositories
{
    public class SessionLogRepository : ISessionLogRepository
    {
        // Inject your DbContext or data source here
        private readonly DKRDbContext _context;

        public SessionLogRepository(DKRDbContext context)
        {
            _context = context;
        }

        public async Task<SessionLog> CreateAsync(SessionLog sessionLog)
        {
            _context.SessionLog.Add(sessionLog);
            await _context.SaveChangesAsync();
            return sessionLog;
        }

        public async Task<SessionLog> GetByIdAsync(string id)
        {
            var sessionLog = await _context.SessionLog.FindAsync(id);
            return sessionLog ?? throw new ArgumentException($"SessionLog with ID {id} not found", nameof(id));
        }

        public async Task<SessionLog?> GetLastSessionLogBySessionIdAsync(string sessionId)
        {
            var sessionLog = await _context.SessionLog
                .Where(x => x.SessionId == sessionId && x.EndTime == null)
                .OrderByDescending(x => x.StartTime)
                .FirstOrDefaultAsync(); // await this

            return sessionLog;
        }

        public async Task<SessionLog> UpdateAsync(SessionLog sessionLog)
        {
            _context.SessionLog.Update(sessionLog);
            await _context.SaveChangesAsync();
            return sessionLog;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var sessionLog = await _context.SessionLog.FindAsync(id);
                if (sessionLog != null)
                {
                    _context.SessionLog.Remove(sessionLog);
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
        public async Task<IEnumerable<SessionLog>> GetBySessionIdAsync(string sessionId)
        {
            return await _context.SessionLog
                .Where(x => x.SessionId == sessionId)
                .ToListAsync();
        }

        public async Task<List<SessionDurationDto>> GetSessionDurationsBySessionIdsAsyncw(List<string> sessionIds)
        {
            var rawDurations = await _context.SessionLog
                  .Where(log => sessionIds.Contains(log.SessionId))
                  .GroupBy(log => log.SessionId)
                  .Select(group => new
                  {
                      SessionId = group.Key,
                      TotalMinutes = group.Sum(log => log.Duration.TotalMinutes)
                  })
                  .ToListAsync();
            return rawDurations
        .Select(x => new SessionDurationDto
        {
            SessionId = x.SessionId,
            TotalDuration = TimeSpan.FromMinutes(x.TotalMinutes)
        })
        .ToList();
        }

        public async Task<List<SessionDurationDto>> GetSessionDurationsBySessionIdsAsync(List<string> sessionIds)
        {
            var logs = await _context.SessionLog

                .Where(log => sessionIds.Contains(log.SessionId))

                .ToListAsync(); // Load logs into memory for Duration calculation

            return logs.GroupBy(log => log.SessionId)

                .Select(group => new SessionDurationDto
                {
                    SessionId = group.Key,

                    TotalDuration = group.Aggregate(TimeSpan.Zero, (sum, log) => sum + log.Duration),

                    LastStartDate = group.Max(log => log.StartTime)  // get latest StartTime

                })

                .ToList();

        }

        //public async Task<List<SessionDurationDto>> GetSessionDurationsBySessionIdsAsync(List<string> sessionIds)
        //{
        //    var logs = await _context.SessionLog
        //        .Where(log => sessionIds.Contains(log.SessionId))
        //        .ToListAsync(); // load logs into memory

        //    return logs
        //        .GroupBy(log => log.SessionId)
        //        .Select(group => new SessionDurationDto
        //        {
        //            SessionId = group.Key,
        //            TotalDuration = TimeSpan.FromMinutes(group.Sum(log => log.Duration.TotalMinutes))
        //        })
        //        .ToList();
        //}
    }
}