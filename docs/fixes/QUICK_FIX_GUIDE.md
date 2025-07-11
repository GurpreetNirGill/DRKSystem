# Quick Fix Guide - Get it Running in 30 Minutes

## 🚨 Immediate Compilation Fixes

### 1. Create Missing Repositories (10 min)

Create `src/DKR.Infrastructure/Repositories/BaseRepository.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using DKR.Infrastructure.Data;

namespace DKR.Infrastructure.Repositories;

public abstract class BaseRepository<T> where T : class
{
    protected readonly DKRDbContext _context;
    protected readonly DbSet<T> _dbSet;

    protected BaseRepository(DKRDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(string id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) return false;
        
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
```

Then create:
- `InventoryRepository.cs` : `BaseRepository<InventoryItem>, IInventoryRepository`
- `HarmReductionRepository.cs` : `BaseRepository<HarmReductionService>, IHarmReductionRepository`

### 2. Fix Service Methods (5 min)

In `ClientService.cs`, add:
```csharp
public async Task<string> GenerateUniqueUuidAsync()
{
    var year = DateTime.Now.Year;
    var count = await _repository.GetCountByYearAsync(year);
    return $"KL-{year}-{(count + 1):D4}";
}
```

In `EmergencyService.cs`, add:
```csharp
public async Task<EmergencyEvent> ReportEmergencyAsync(EmergencyEvent emergency)
{
    emergency.Id = Guid.NewGuid().ToString();
    emergency.OccurredAt = DateTime.UtcNow;
    
    // Send notifications
    await _notificationService.SendAsync(new NotificationRequest
    {
        Channel = NotificationChannel.SMS,
        Subject = "NOTFALL DKR",
        Message = $"Notfall: {emergency.Type} in Raum {emergency.Room}"
    });
    
    return await _repository.CreateAsync(emergency);
}
```

### 3. Register Services in DI (5 min)

In `ServiceCollectionExtensions.cs`, add:
```csharp
// Repositories
services.AddScoped<IClientRepository, ClientRepository>();
services.AddScoped<ISessionRepository, SessionRepository>();
services.AddScoped<IInventoryRepository, InventoryRepository>();
services.AddScoped<IHarmReductionRepository, HarmReductionRepository>();

// Services
services.AddScoped<ClientService>();
services.AddScoped<SessionService>();
services.AddScoped<EmergencyService>();
services.AddScoped<InventoryService>();
services.AddScoped<HarmReductionService>();
```

### 4. Fix Connection String (2 min)

In `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DKRSystem;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 5. Run Migrations (3 min)

```bash
# In project root
dotnet ef migrations add InitialCreate --project src/DKR.Infrastructure --startup-project src/DKR.Web
dotnet ef database update --project src/DKR.Infrastructure --startup-project src/DKR.Web
```

## ✅ That's it! 

Now run:
```bash
dotnet run --project src/DKR.Web
```

Navigate to: https://localhost:5001

---

## 🎯 Next Steps After It Compiles:

1. **Add Seed Data** for testing
2. **Implement Authentication** (currently missing)
3. **Complete UI Components** (some are stubbed)
4. **Add Validation** to forms
5. **Implement SignalR** for real-time updates
6. **Add Logging** throughout
7. **Write Tests** for critical paths

Good luck! 🚀