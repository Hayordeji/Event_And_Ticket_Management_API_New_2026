namespace TicketingSystem.Modules.Fulfillment.Application.Services
{
    /// <summary>
    /// Fetches attendee information for event-related notifications
    /// Attendees are determined from tickets issued for an event
    /// </summary>
    public interface IAttendeeNotificationService
    {
        /// <summary>
        /// Gets distinct attendees who have valid tickets for an event
        /// </summary>
        Task<List<AttendeeDto>> GetAttendeesByEventIdAsync(
            Guid eventId,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// DTO containing attendee information for notifications
    /// </summary>
    public record AttendeeDto(
        string Email,
        string Name,
        string OrderNumber);
}