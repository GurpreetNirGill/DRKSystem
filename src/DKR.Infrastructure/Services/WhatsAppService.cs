using DKR.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DKR.Infrastructure.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        public Task SendEmergencyWhatsAppAsync(List<string> phoneNumbers, string message)
        {
            // TODO: Implement WhatsApp sending logic here
            return Task.CompletedTask;
        }

        public Task SendAppointmentReminderAsync(string phoneNumber, string message)
        {
            // TODO: Implement WhatsApp sending logic here
            return Task.CompletedTask;
        }
    }
}