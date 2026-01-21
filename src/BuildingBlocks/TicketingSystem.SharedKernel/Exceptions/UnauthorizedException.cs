using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.SharedKernel.Exceptions
{
     ///<summary>
/// Exception thrown when user is not authenticated
/// </summary>
    public class UnauthorizedException : DomainException
    {
        public UnauthorizedException(string message = "Unauthorized access.")
        : base(message)
        {
        }
    }
}
