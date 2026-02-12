using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Domain.Events;
using TicketingSystem.Modules.Finance.Application.Services;

namespace TicketingSystem.Modules.Finance.Application.EventHandlers
{
    /// <summary>
    /// Handles EventPublishedEvent from Catalog module to pre-create host ledger account
    /// This is the PRIMARY defense against race conditions - creates account BEFORE any orders
    /// </summary>
    public class EventPublishedEventHandler : INotificationHandler<EventPublishedEvent>
    {
        private readonly IHostAccountService _hostAccountService;
        private readonly ILogger<EventPublishedEventHandler> _logger;

        public EventPublishedEventHandler(
            IHostAccountService hostAccountService,
            ILogger<EventPublishedEventHandler> logger)
        {
            _hostAccountService = hostAccountService;
            _logger = logger;
        }

        public async Task Handle(EventPublishedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Processing EventPublishedEvent for event {EventName}. Creating host ledger account for HostId={HostId}",
                notification.EventName, notification.HostId);

            try
            {
                // Pre-create host account (idempotent - safe to call multiple times)
                var accountCode = await _hostAccountService.EnsureHostAccountExistsAsync(
                    notification.HostId,
                    $"Host-{notification.HostId.ToString()[..8]}", // TODO: Get actual host name from Identity module
                    cancellationToken);

                _logger.LogInformation(
                    "Host ledger account ready: {AccountCode} for event {EventName}",
                    accountCode, notification.EventName);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating host ledger account for event {EventId}, HostId={HostId}",
                    notification.EventId, notification.HostId);

                // Don't throw - event handlers should be resilient
                // Account will be created during payment as backup
            }
        }
    }
}
