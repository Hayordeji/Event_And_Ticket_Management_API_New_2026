using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Access.Domain.Enums;

namespace TicketingSystem.Modules.Access.Application.Services
{
    public interface ITicketValidationService
    {
        Task<TicketValidationResult> ValidateAsync(
        string qrCodeData,
        Guid eventId,
        CancellationToken cancellationToken = default);
    }

    public sealed record TicketValidationResult(
    bool IsValid,
    Guid? TicketId,
    string? TicketNumber,
    string? TicketTypeName,
    string? CustomerName,
    DenialReason? DenialReason,
    string? DenialMessage);





}
