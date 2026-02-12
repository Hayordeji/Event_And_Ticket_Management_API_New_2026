using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Fulfillment.Application.Commands
{
    public sealed record GenerateTicketsCommand(
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    string CustomerEmail,
    string CustomerFirstName,
    string CustomerLastName,
    Guid EventId,
    string EventName,
    DateTime EventStartDate,
    DateTime EventEndDate,
    string VenueName,
    string VenueAddress,
    string VenueCity,
    List<GenerateTicketItemDto> Items) : IRequest<Result<List<Guid>>>;


    public sealed record GenerateTicketItemDto(
    Guid TicketTypeId,
    string TicketTypeName,
    int Quantity,
    decimal UnitPrice,
    string Currency);
}
