using DKR.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;

public class EmailService : IEmailService
{
    public Task SendEmergencyEmailAsync(List<string> recipients, string subject, string message)
    {
        // Implement your email sending logic here
        return Task.CompletedTask;
    }

    public Task SendReportEmailAsync(string recipient, string subject, string htmlContent, byte[]? attachment = null)
    {
        // Implement your email sending logic here
        return Task.CompletedTask;
    }
}