using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Fulfillment.Application.DTOs
{
    public class SendEmailResponse
    {
        public bool IsSuccess { get; set; }
        public string MessageId { get; set; }
        public string Response { get; set; }
    }
}
