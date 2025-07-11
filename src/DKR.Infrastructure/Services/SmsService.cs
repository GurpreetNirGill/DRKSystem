using DKR.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DKR.Infrastructure.Services
{
    public class SmsService : ISMSService
    {
        public Task SendEmergencySMSAsync(List<string> phoneNumbers, string message)
        {
            // TODO: Implement actual SMS sending logic
            return Task.CompletedTask;
        }

        public Task SendReminderSMSAsync(string phoneNumber, string message)
        {
            // TODO: Implement actual SMS sending logic
            return Task.CompletedTask;
        }
    }
}