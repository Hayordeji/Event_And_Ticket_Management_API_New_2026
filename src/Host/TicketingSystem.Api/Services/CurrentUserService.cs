using System.Security.Claims;
using TicketingSystem.Modules.Identity.Domain.Entities;
using TicketingSystem.SharedKernel.Services;

namespace TicketingSystem.Api.Services
{
    /// <summary>
    /// Extracts current user context from HttpContext claims
    /// Claims populated by authentication middleware
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("userId")?.Value;
                return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
            }
        }

        public string Role
        {
            get
            {
                var roleClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role);
                return roleClaim?.Value ?? string.Empty;
            }
        }

        public bool IsAdmin()
        {
            return Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }
    }
}
