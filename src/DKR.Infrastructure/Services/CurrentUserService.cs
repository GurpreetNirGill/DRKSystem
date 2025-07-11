using DKR.Core.Interfaces;
using System.Collections.Generic;

namespace DKR.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public string GetCurrentUserId()
        {
            // TODO: Implement actual user context logic
            return "system";
        }

        public string? GetCurrentTenantId()
        {
            // TODO: Implement actual tenant context logic
            return null;
        }

        public string GetCurrentUserName()
        {
            // TODO: Implement actual user context logic
            return "System";
        }

        public List<string> GetCurrentUserRoles()
        {
            // TODO: Implement actual user roles logic
            return new List<string> { "Admin" };
        }
    }
}