using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.SharedKernel.Exceptions
{
    /// <summary>
/// Exception thrown when user lacks permissions
/// </summary>
    public class ForbiddenException : DomainException
    {
        public ForbiddenException(string message = "Access forbidden.")
        : base(message)
        {
        }
    }
}
