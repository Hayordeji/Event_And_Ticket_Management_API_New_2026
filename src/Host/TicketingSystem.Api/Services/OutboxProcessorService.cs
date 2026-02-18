using MediatR;
using System.Text.Json;
using TicketingSystem.SharedKernel.Outbox;

namespace TicketingSystem.Api.Services
{
    /// <summary>
    /// Background worker that processes outbox messages from all modules.
    /// Runs every 5 seconds, reads pending messages, dispatches them via MediatR,
    /// and handles retry/dead-letter logic.
    /// </summary>
    public class OutboxProcessorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxProcessorService> _logger;

        public OutboxProcessorService(
            IServiceProvider serviceProvider,
            ILogger<OutboxProcessorService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OutboxProcessorService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxMessagesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error in OutboxProcessorService.");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }

            _logger.LogInformation("OutboxProcessorService stopped.");
        }

        private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            // Get all outbox repositories (one per module)
            var repositories = scope.ServiceProvider
                .GetServices<IOutboxMessageRepository>()
                .ToList();

            if (!repositories.Any())
            {
                _logger.LogWarning("No outbox repositories registered. Skipping cycle.");
                return;
            }

            foreach (var repository in repositories)
            {
                var messages = await repository.GetPendingMessagesAsync(
                    batchSize: 20,
                    ct: cancellationToken);

                if (!messages.Any())
                    continue;

                _logger.LogInformation(
                    "Processing {Count} outbox messages from {RepositoryType}",
                    messages.Count,
                    repository.GetType().Name);

                foreach (var message in messages)
                {
                    await ProcessMessageAsync(message, repository, scope, cancellationToken);
                }
            }
        }

        private async Task ProcessMessageAsync(
            OutboxMessage message,
            IOutboxMessageRepository repository,
            IServiceScope scope,
            CancellationToken cancellationToken)
        {
            try
            {
                // Step 1: Deserialize the event
                var eventType = Type.GetType(message.EventType);
                if (eventType == null)
                {
                    _logger.LogError(
                        "Cannot resolve event type {EventType} for message {MessageId}. Moving to dead letter.",
                        message.EventType,
                        message.Id);

                    message.RecordFailure($"Cannot resolve event type: {message.EventType}", maxRetries: 0);
                    await repository.UpdateAsync(message, cancellationToken);
                    return;
                }

                var domainEvent = JsonSerializer.Deserialize(message.EventPayload, eventType) as INotification;
                if (domainEvent == null)
                {
                    _logger.LogError(
                        "Deserialized event is not an INotification for message {MessageId}. Moving to dead letter.",
                        message.Id);

                    message.RecordFailure("Deserialized event is not an INotification.", maxRetries: 0);
                    await repository.UpdateAsync(message, cancellationToken);
                    return;
                }

                // Step 2: Dispatch to MediatR handlers
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Publish(domainEvent, cancellationToken);

                // Step 3: Mark as processed
                message.MarkAsProcessed();
                await repository.UpdateAsync(message, cancellationToken);

                _logger.LogInformation(
                    "Successfully processed outbox message {MessageId} ({EventType})",
                    message.Id,
                    eventType.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process outbox message {MessageId} (Attempt {RetryCount}): {Error}",
                    message.Id,
                    message.RetryCount + 1,
                    ex.Message);

                message.RecordFailure(ex.Message, maxRetries: 5);
                await repository.UpdateAsync(message, cancellationToken);

                if (message.Status == OutboxMessageStatus.DeadLettered)
                {
                    _logger.LogCritical(
                        "Outbox message {MessageId} moved to dead letter after {RetryCount} retries. Manual intervention required.",
                        message.Id,
                        message.RetryCount);
                }
            }
        }
    }
}
