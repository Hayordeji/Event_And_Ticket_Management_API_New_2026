using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TicketingSystem.Modules.Catalog.Application.Commands;
using TicketingSystem.Modules.Catalog.Application.DTOs;
using TicketingSystem.Modules.Catalog.Application.Queries;
using TicketingSystem.Modules.Catalog.Domain.DTOs;
using TicketingSystem.SharedKernel.ApiResponses;
using TicketingSystem.SharedKernel.Authorization;
using TicketingSystem.SharedKernel.Exceptions;

namespace TicketingSystem.Modules.Catalog.Api.Controllers
{
    /// <summary>
    /// Events API controller for managing event lifecycle and ticket types
    /// </summary>
    [Route("api/catalog/events")]
    [ApiController]
    [Authorize]
    public class EventsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EventsController> _logger;

        public EventsController(IMediator mediator, ILogger<EventsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Create a new event
        /// </summary>
        /// <param name="request">Event creation details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created event ID</returns>
        [HttpPost]
        [EnableRateLimiting("fixed_create_endpoints")]
        [Authorize(Policy = PolicyNames.RequireHost)]        // Only hosts create events
        public async Task<IActionResult> CreateEvent(
            [FromBody] CreateEventRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!Guid.TryParse(User.FindFirst("userId")?.Value, out var hostId))
            {
                return Unauthorized();
            }


            var command = new CreateEventCommand(hostId, request);

            try
            {
                var result = await _mediator.Send(command, cancellationToken);

                if (result.IsFailure)
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        result.Error,
                        traceId: HttpContext.TraceIdentifier));

                var eventId = result.Value;

                _logger.LogInformation("Event created successfully: {EventId} by host {HostId}", eventId, hostId);

                return CreatedAtAction(
                    nameof(GetEvent),
                    new { id = eventId },
                    ApiResponse<object>.SuccessResponse(
                        new { id = eventId },
                        HttpContext.TraceIdentifier));
            }
            catch (ConflictException ex)
            {
                _logger.LogWarning("Conflict creating event: {Message}", ex.Message);
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ex.Message,
                    traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event");
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "An error occurred while creating the event",
                    traceId: HttpContext.TraceIdentifier));
            }
        }

        /// <summary>
        /// Get event by ID
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Event details</returns>
        [HttpGet("{id:guid}")]
        [EnableRateLimiting("fixed_get_endpoints")]
        [AllowAnonymous]       
        public async Task<IActionResult> GetEvent(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var query = new GetEventByIdQuery(id);

            try
            {
                var result = await _mediator.Send(query, cancellationToken);

                if (result == null)
                    return NotFound(ApiResponse<object>.ErrorResponse(
                        "Event not found",
                        traceId: HttpContext.TraceIdentifier));

                return Ok(ApiResponse<EventResponse>.SuccessResponse(result, HttpContext.TraceIdentifier));
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Event not found: {EventId}", id);
                return NotFound(ApiResponse<object>.ErrorResponse(
                    ex.Message,
                    traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving event: {EventId}", id);
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "An error occurred while retrieving the event",
                    traceId: HttpContext.TraceIdentifier));
            }
        }

        /// <summary>
        /// Get all tickets for the authenticated customer
        /// </summary>
        [HttpGet("my-events")]
        [EnableRateLimiting("fixed_get_endpoints")]
        [Authorize(Policy = PolicyNames.RequireHost)]
        public async Task<IActionResult> GetMyEvents([FromQuery] SearchHostEventsRequest request, CancellationToken cancellationToken)
        {

            if (!Guid.TryParse(User.FindFirst("userId")?.Value, out var hostId))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _mediator.Send(
                    new SearchHostEventsQueryCommand(request, hostId),
                    cancellationToken);

                if (result.IsFailure)
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        result.Error,
                        traceId: HttpContext.TraceIdentifier));

                return Ok(ApiResponse<object>.SuccessResponse(
                    result.Value.Events,
                    HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching events");
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "An error occurred while searching for events",
                    traceId: HttpContext.TraceIdentifier));
            }
        }



        /// <summary>
        /// Publish an event
        /// </summary>
        /// <param name="request">Event publish details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created event ID</returns>
        [HttpPut("publish/{id:guid}")]
        [EnableRateLimiting("fixed_create_endpoints")]
        [Authorize(Policy = PolicyNames.RequireHost)]        

        public async Task<IActionResult> PublishEvent(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            if (!Guid.TryParse(User.FindFirst("userId")?.Value, out var hostId))
            {
                return Unauthorized();
            }

            var command = new PublishEventCommand(id, hostId);

            try
            {
                var result = await _mediator.Send(command, cancellationToken);

                if (result.IsFailure)
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        result.Error,
                        traceId: HttpContext.TraceIdentifier));

                _logger.LogInformation("Event published successfully: {EventId} by host {HostId}", id, hostId);

                return Ok(result);
            }
            catch (ConflictException ex)
            {
                _logger.LogWarning("Conflict creating event: {Message}", ex.Message);
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ex.Message,
                    traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event");
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "An error occurred while creating the event",
                    traceId: HttpContext.TraceIdentifier));
            }
        }

        /// <summary>
        /// Update an event
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <param name="request">Updated event details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>No content on success</returns>
        [HttpPut("{id:guid}")]
        [EnableRateLimiting("fixed_create_endpoints")]
        [Authorize(Policy = PolicyNames.RequireHost)]
        public async Task<IActionResult> UpdateEvent(
            Guid id,
            [FromBody] UpdateEventRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!Guid.TryParse(User.FindFirst("userId")?.Value, out var hostId))
            {
                return Unauthorized();
            }

            var command = new UpdateEventCommand(id, hostId, request);

            try
            {
                var result = await _mediator.Send(command, cancellationToken);

                if (result.IsFailure)
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        result.Error,
                        traceId: HttpContext.TraceIdentifier));

                _logger.LogInformation("Event updated successfully: {EventId} by host {HostId}", id, hostId);

                return Ok(ApiResponse<object>.SuccessResponse(
                    new { id = id, message = "Event updated successfully" },
                    HttpContext.TraceIdentifier));
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Event not found for update: {EventId}", id);
                return NotFound(ApiResponse<object>.ErrorResponse(
                    ex.Message,
                    traceId: HttpContext.TraceIdentifier));
            }
            catch (ConflictException ex)
            {
                _logger.LogWarning("Conflict updating event: {Message}", ex.Message);
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ex.Message,
                    traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event: {EventId}", id);
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "An error occurred while updating the event",
                    traceId: HttpContext.TraceIdentifier));
            }
        }

        /// <summary>
        /// Search events with filters and pagination
        /// </summary>
        /// <param name="searchTerm">Search term for event name/description</param>
        /// <param name="city">Filter by city</param>
        /// <param name="startDateFrom">Filter events starting from this date</param>
        /// <param name="startDateTo">Filter events ending before this date</param>
        /// <param name="pageNumber">Page number (1-based, default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of events</returns>
        [HttpGet("search")]
        [EnableRateLimiting("fixed_get_endpoints")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchEvents(
            [FromQuery] SearchEventsRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _mediator.Send(
                    new SearchEventsQueryCommand(request),
                    cancellationToken);

                if (result.IsFailure)
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        result.Error,
                        traceId: HttpContext.TraceIdentifier));

                return Ok(ApiResponse<object>.SuccessResponse(
                    result.Value.Events,
                    HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching events");
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "An error occurred while searching for events",
                    traceId: HttpContext.TraceIdentifier));
            }
        }

        /// <summary>
        /// Add a ticket type to an event
        /// </summary>
        /// <param name="eventId">Event ID</param>
        /// <param name="request">Ticket type creation details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created ticket type ID</returns>
        [HttpPost("{eventId:guid}/ticket-types")]
        [EnableRateLimiting("fixed_create_endpoints")]
        [Authorize(Policy = PolicyNames.RequireHost)]

        public async Task<IActionResult> AddTicketType(
            Guid eventId,
            [FromBody] AddTicketTypeRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!Guid.TryParse(User.FindFirst("userId")?.Value, out var hostId))
            {
                return Unauthorized();
            }

            var command = new AddTicketTypeCommand(eventId, hostId, request);

            try
            {
                var result = await _mediator.Send(command, cancellationToken);

                if (result.IsFailure)
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        result.Error,
                        traceId: HttpContext.TraceIdentifier));

                var ticketTypeId = result.Value;

                _logger.LogInformation(
                    "Ticket type added successfully: {TicketTypeId} to event {EventId}",
                    ticketTypeId,
                    eventId);

                return CreatedAtAction(
                    nameof(GetTicketTypesByEvent),
                    new { eventId = eventId },
                    ApiResponse<object>.SuccessResponse(
                        new { id = ticketTypeId },
                        HttpContext.TraceIdentifier));
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Event not found for ticket type: {EventId}", eventId);
                return NotFound(ApiResponse<object>.ErrorResponse(
                    ex.Message,
                    traceId: HttpContext.TraceIdentifier));
            }
            catch (ConflictException ex)
            {
                _logger.LogWarning("Conflict adding ticket type: {Message}", ex.Message);
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ex.Message,
                    traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding ticket type to event: {EventId}", eventId);
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "An error occurred while adding the ticket type",
                    traceId: HttpContext.TraceIdentifier));
            }
        }

        /// <summary>
        /// Get all ticket types for an event
        /// </summary>
        /// <param name="eventId">Event ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of ticket types</returns>
        [HttpGet("{eventId:guid}/ticket-types")]
        [EnableRateLimiting("fixed_get_endpoints")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTicketTypesByEvent(
            Guid eventId,
            CancellationToken cancellationToken = default)
        {
            var query = new GetTicketTypesByEventQuery(eventId);

            try
            {
                var result = await _mediator.Send(query, cancellationToken);

                if (result.IsFailure)
                    return NotFound(ApiResponse<object>.ErrorResponse(
                        result.Error,
                        traceId: HttpContext.TraceIdentifier));

                return Ok(ApiResponse<List<TicketTypeResponse>>.SuccessResponse(
                    result.Value,
                    HttpContext.TraceIdentifier));
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Event not found for ticket types: {EventId}", eventId);
                return NotFound(ApiResponse<object>.ErrorResponse(
                    ex.Message,
                    traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket types for event: {EventId}", eventId);
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "An error occurred while retrieving ticket types",
                    traceId: HttpContext.TraceIdentifier));
            }
        }

        /// <summary>
        /// Health check for catalog/events module
        /// </summary>
        /// <returns>Health status</returns>
        /// <response code="200">Service is healthy</response>
        [HttpGet("health")]
        [EnableRateLimiting("fixed_get_endpoints")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok(new
            {
                module = "Catalog.Events",
                status = "healthy",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
