using DKR.Core.Entities;
using DKR.Core.Interfaces;
using DKR.Shared.Enums;
using System.Data;

namespace DKR.Core.Services;

public class HarmReductionService
{
    private readonly IHarmReductionRepository _repository;
    private readonly INotificationService _notificationService;

    public HarmReductionService(IHarmReductionRepository repository, INotificationService notificationService)
    {
        _repository = repository;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<HarmReduction>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<HarmReduction?> GetByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<HarmReduction> CreateAsync(HarmReduction service)
    {
        service.Id = Guid.NewGuid().ToString();
        service.CreatedAt = DateTime.UtcNow;
        return await _repository.CreateAsync(service);
    }

    public async Task<HarmReduction> UpdateAsync(HarmReduction service)
    {
        service.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(service);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<HarmReduction>> GetByClientIdAsync(string clientId)
    {
        return await _repository.GetByClientIdAsync(clientId);
    }

    public async Task<IEnumerable<HarmReduction>> GetPendingAppointmentsAsync()
    {
        var all = await _repository.GetAllAsync();
        return all.Where(s => s.ServiceDate > DateTime.UtcNow && !s.IsCompleted);
    }

    public async Task<HarmReduction> BookAppointmentAsync(string clientId, Entities.ServiceType serviceType, DateTime appointmentDate)
    {
        var appointment = new HarmReduction
        {
            Id = Guid.NewGuid().ToString(),
            ClientId = clientId,
            Type = serviceType,
            ServiceDate = appointmentDate,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(appointment);

        // Send reminder notification
        await _notificationService.SendAsync(new NotificationRequest
        {
            Channel = NotificationChannel.Email,
            Subject = $"Termin: {serviceType}",
            Message = $"Ihr Termin am {appointmentDate:dd.MM.yyyy HH:mm}"
        });

        return created;
    }

    public async Task<HarmReduction> CompleteAppointmentAsync(string appointmentId, string? result = null)
    {
        var appointment = await _repository.GetByIdAsync(appointmentId);
        if (appointment == null)
            throw new ArgumentException("Appointment not found");

        appointment.IsCompleted = true;
        appointment.Result = result;
        appointment.UpdatedAt = DateTime.UtcNow;

        return await _repository.UpdateAsync(appointment);
    }

    public async Task<bool> SendReminderAsync(string appointmentId)
    {
        var appointment = await _repository.GetByIdAsync(appointmentId);
        if (appointment == null) return false;

        await _notificationService.SendAsync(new NotificationRequest
        {
            Channel = NotificationChannel.SMS,
            Subject = "Termin-Erinnerung",
            Message = $"Erinnerung: Ihr Termin morgen um {appointment.ServiceDate:HH:mm}"
        });

        return true;
    }

    public async Task<Dictionary<Entities.ServiceType, int>> GetMonthlyStatisticsAsync(DateTime month)
    {
        var services = await _repository.GetAllAsync();
        return services
            .Where(s => s.ServiceDate.Month == month.Month && s.ServiceDate.Year == month.Year)
            .GroupBy(s => s.Type)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<IEnumerable<HarmReduction>> GetAppointmentsByDateAsync(DateTime date)
    {
        var services = await _repository.GetAllAsync();
        return services.Where(s => s.ServiceDate.Date == date.Date);
    }

    public async Task<HarmReduction> DocumentWoundCareAsync(string clientId, string notes)
    {
        var service = new HarmReduction
        {
            Id = Guid.NewGuid().ToString(),
            ClientId = clientId,
            Type = Entities.ServiceType.WoundCare,
            ServiceDate = DateTime.UtcNow,
            Notes = notes,
            IsCompleted = true,
            CreatedAt = DateTime.UtcNow
        };

        return await _repository.CreateAsync(service);
    }

    public async Task<IEnumerable<HarmReduction>> GetServicesByDateRangeAsync(DateTime from, DateTime to)
    {
        var services = await _repository.GetServicesByDateRangeAsync(from, to);
        return services;
    }
}