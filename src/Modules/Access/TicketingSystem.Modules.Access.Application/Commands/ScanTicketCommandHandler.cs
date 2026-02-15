using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Access.Application.DTOs;
using TicketingSystem.Modules.Access.Application.Services;
using TicketingSystem.Modules.Access.Domain.Entities;
using TicketingSystem.Modules.Access.Domain.Enums;
using TicketingSystem.Modules.Access.Domain.Repositories;
using TicketingSystem.Modules.Access.Infrastructure.Persistence;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Access.Application.Commands
{
    public class ScanTicketCommandHandler : IRequestHandler<ScanTicketCommand, Result<ScanTicketResponse>>
    {
        private readonly IScanLogRepository _scanLogRepository;
        private readonly ITicketValidationService _ticketValidationService;
        private readonly AccessDbContext _context;
        private readonly ITicketStatusService _ticketStatusService;

        private readonly ILogger<ScanTicketCommandHandler> _logger;

        public ScanTicketCommandHandler(
            IScanLogRepository scanLogRepository,
            ITicketValidationService ticketValidationService,
            AccessDbContext context,
            ILogger<ScanTicketCommandHandler> logger,
            ITicketStatusService ticketStatusService)
        {
            _scanLogRepository = scanLogRepository;
            _ticketValidationService = ticketValidationService;
            _context = context;
            _logger = logger;
            _ticketStatusService = ticketStatusService;
        }

        public async Task<Result<ScanTicketResponse>> Handle(
            ScanTicketCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
               "Processing ticket scan. EventId={EventId}, Device={DeviceId}, Gate={Gate}",
               request.EventId, request.DeviceId, request.GateLocation);

                // Step 1: Validate the ticket via Fulfillment module
                var validation = await _ticketValidationService.ValidateAsync(
                    request.QrCodeData,
                    request.EventId,
                    cancellationToken);

                ScanLog scanLog;

                if (!validation.IsValid)
                {
                    _logger.LogWarning(
                        "Ticket scan DENIED. TicketNumber={TicketNumber}, Reason={Reason}",
                        validation.TicketNumber.ToString() ?? "Unknown", validation.DenialReason.ToString());


                    // Record denied scan
                    scanLog = ScanLog.RecordDenied(
                        ticketId: validation.TicketId ?? Guid.Empty,
                        ticketNumber: validation.TicketNumber ?? "UNKNOWN",
                        eventId: request.EventId,
                        scannedBy: request.ScannedBy,
                        deviceId: request.DeviceId,
                        gateLocation: request.GateLocation,
                        reason: validation.DenialReason!.Value);

                    await _scanLogRepository.AddAsync(scanLog, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    return Result.Success(new ScanTicketResponse(
                        IsAllowed: false,
                        TicketNumber: validation.TicketNumber ?? "UNKNOWN",
                        TicketTypeName: validation.TicketTypeName,
                        CustomerName: validation.CustomerName,
                        DenialReason: validation.DenialReason?.ToString(),
                        DenialMessage: validation.DenialMessage,
                        ScannedAt: DateTime.UtcNow));
                }

                // Step 2: Check for duplicate scan (anti-fraud)
                var alreadyScanned = await _scanLogRepository.HasBeenScannedSuccessfullyAsync(
                    validation.TicketId!.Value,
                    cancellationToken);

                if (alreadyScanned)
                {
                    _logger.LogWarning(
                        "FRAUD DETECTED: Ticket {TicketNumber} already scanned!",
                        validation.TicketNumber);

                    scanLog = ScanLog.RecordDenied(
                        ticketId: validation.TicketId!.Value,
                        ticketNumber: validation.TicketNumber!,
                        eventId: request.EventId,
                        scannedBy: request.ScannedBy,
                        deviceId: request.DeviceId,
                        gateLocation: request.GateLocation,
                        reason: DenialReason.AlreadyUsed);

                    await _scanLogRepository.AddAsync(scanLog, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    return Result.Success(new ScanTicketResponse(
                        IsAllowed: false,
                        TicketNumber: validation.TicketNumber!,
                        TicketTypeName: validation.TicketTypeName,
                        CustomerName: validation.CustomerName,
                        DenialReason: DenialReason.AlreadyUsed.ToString(),
                        DenialMessage: "This ticket has already been used for entry.",
                        ScannedAt: DateTime.UtcNow));
                }

                // Step 3: Allow entry - record successful scan
                _logger.LogInformation(
                    "Ticket scan ALLOWED. TicketNumber={TicketNumber}, Customer={CustomerName}",
                    validation.TicketNumber, validation.CustomerName);

                scanLog = ScanLog.RecordAllowed(
                    ticketId: validation.TicketId!.Value,
                    ticketNumber: validation.TicketNumber!,
                    eventId: request.EventId,
                    scannedBy: request.ScannedBy,
                    deviceId: request.DeviceId,
                    gateLocation: request.GateLocation);

                await _ticketStatusService.MarkAsUsedAsync(
                validation.TicketId!.Value,
                request.ScannedBy,
                request.GateLocation,
                cancellationToken);

                await _scanLogRepository.AddAsync(scanLog, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return Result.Success(new ScanTicketResponse(
                    IsAllowed: true,
                    TicketNumber: validation.TicketNumber!,
                    TicketTypeName: validation.TicketTypeName,
                    CustomerName: validation.CustomerName,
                    DenialReason: null,
                    DenialMessage: null,
                    ScannedAt: DateTime.UtcNow));

            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
