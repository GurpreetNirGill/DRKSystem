using DKR.Core.Entities;
using DKR.Core.Interfaces;
using DKR.Infrastructure.Data;
using DKR.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace DKR.Infrastructure.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly DKRDbContext _context;

    public SessionRepository(DKRDbContext context)
    {
        _context = context;
    }

    public async Task<Session> CreateAsync(Session session)
    {
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<Session> GetByIdAsync(string sessionId)
    {
        var session = await _context.Sessions
            .Include(s => s.Client)
            .Include(s => s.EmergencyEvent)
             .Include(s => s.Supply)
              .Include(s => s.SessionLogs)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
        return session ?? throw new ArgumentException($"Session with ID {sessionId} not found", nameof(sessionId));
    }

    public async Task<IEnumerable<Session>> GetActiveSessionsAsync()
    {
        return await _context.Sessions
            .Include(s => s.Client)
            .Include(s => s.EmergencyEvent)
            .Where(s => s.Status == SessionStatus.Active || s.Status == SessionStatus.Monitoring)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }
    public async Task<IEnumerable<Session>> GetActiveAndPauseSessionsAsync()
    {
        return await _context.Sessions
            .Where(s => s.Status == SessionStatus.Active || s.Status == SessionStatus.Monitoring || s.Status == SessionStatus.Pause)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetPauseSessionsAsync()
    {
        return await _context.Sessions
            .Include(s => s.Client)
            .Where(s => s.Status == SessionStatus.Pause)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }
    public async Task<IEnumerable<Session>> GetActiveStatusSessionsAsync()
    {
        return await _context.Sessions
            .Include(s => s.Client)
            .Include(s => s.EmergencyEvent)
            .Where(s => s.Status == SessionStatus.Active || s.Status == SessionStatus.Monitoring)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetActiveSessionsByRoomAsync(string room)
    {
        return await _context.Sessions
            .Include(s => s.Client)
            .Include(s => s.EmergencyEvent)
            .Where(s => s.Room == room && 
                       (s.Status == SessionStatus.Active || s.Status == SessionStatus.Monitoring))
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<int> GetActiveSessionsCountAsync()
    {
        return await _context.Sessions
            .CountAsync(s => s.Status == SessionStatus.Active || s.Status == SessionStatus.Monitoring);
    }

    public async Task<bool> IsRoomAvailableAsync(string room)
    {
        return !await _context.Sessions
            .AnyAsync(s => s.Room == room && 
                          (s.Status == SessionStatus.Active || s.Status == SessionStatus.Monitoring));
    }

    public async Task<Session?> GetActiveSessionByRoomAsync(string room)
    {
        return await _context.Sessions
            .Include(s => s.Client)
            .FirstOrDefaultAsync(s => s.Room == room && 
                                     (s.Status == SessionStatus.Active || s.Status == SessionStatus.Monitoring));
    }

    public async Task<IEnumerable<Session>> GetByClientIdAsync(string clientId)
    {
        return await _context.Sessions
            .Include(s => s.Client)
            .Where(s => s.ClientId == clientId)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetSessionsByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _context.Sessions
            .Include(s => s.Client)
            .Where(s => s.StartTime >= from && s.StartTime <= to)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetEmergencySessionsAsync()
    {
        return await _context.Sessions
            .Include(s => s.Client)
             .Include(s => s.EmergencyEvent)
            .Where(s => s.Status == SessionStatus.Emergency)
            .OrderByDescending(s => s.EmergencyEvent.OccurredAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetSessionsByStatusAsync(SessionStatus status)
    {
        return await _context.Sessions
            .Include(s => s.Client)
            .Where(s => s.Status == status)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<Session> UpdateAsync(Session session)
    {
        _context.Sessions.Update(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<bool> DeleteAsync(string sessionId)
    {
        try
        {
            var session = await _context.Sessions.FindAsync(sessionId);
            if (session != null)
            {
                _context.Sessions.Remove(session);
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

    public async Task<IEnumerable<Session>> GetLongRunningSessionsAsync(int maxMinutes = 30)
    {
        var cutoffTime = DateTime.UtcNow.AddMinutes(-maxMinutes);
        return await _context.Sessions
            .Include(s => s.Client)
            .Where(s => (s.Status == SessionStatus.Active || s.Status == SessionStatus.Monitoring) && 
                       s.StartTime <= cutoffTime)
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetRoomOccupancyAsync()
    {
        var activeSessions = await _context.Sessions
            .Where(s => s.Status == SessionStatus.Active || s.Status == SessionStatus.Monitoring)
            .GroupBy(s => s.Room)
            .Select(g => new { Room = g.Key, Count = g.Count() })
            .ToListAsync();

        return activeSessions.ToDictionary(x => x.Room, x => x.Count);
    }

    public async Task<IEnumerable<Session>> GetTodaysSessionsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await _context.Sessions
            .Include(s => s.Client)
            .Where(s => s.StartTime >= today && s.StartTime < tomorrow)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<int> GetSessionsCountByDateAsync(DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await _context.Sessions
            .CountAsync(s => s.StartTime >= startOfDay && s.StartTime < endOfDay);
    }

    public async Task<TimeSpan> GetAverageSessionDurationAsync()
    {
        var completedSessions = await _context.Sessions
            .Where(s => s.Status == SessionStatus.Completed && s.EndTime.HasValue)
            .Select(s => new { 
                Duration = s.EndTime!.Value - s.StartTime 
            })
            .ToListAsync();

        if (!completedSessions.Any())
            return TimeSpan.Zero;

        var totalTicks = completedSessions.Sum(s => s.Duration.Ticks);
        var averageTicks = totalTicks / completedSessions.Count;
        
        return new TimeSpan(averageTicks);
    }
}