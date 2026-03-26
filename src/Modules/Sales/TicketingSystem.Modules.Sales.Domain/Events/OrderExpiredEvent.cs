using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Domain.Events
{
    public record OrderExpiredEvent(
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId, 
    List<ExpiredOrderItem> Items,  
    DateTime ExpiredAt
    ) : DomainEvent;
    
    public record ExpiredOrderItem(
        Guid TicketTypeId,
        int Quantity
    );
}
