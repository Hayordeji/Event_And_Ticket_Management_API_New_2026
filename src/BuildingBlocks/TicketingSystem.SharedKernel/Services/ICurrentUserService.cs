namespace TicketingSystem.SharedKernel.Services
{
    /// <summary>
    /// Provides access to the current user's context (UserId, Role)
    /// Implemented in Host project and injected into handlers
    /// </summary>
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        string Role { get; }
        bool IsAdmin();
    }
}