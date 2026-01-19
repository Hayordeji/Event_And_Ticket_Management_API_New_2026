using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.SharedKernel.Exceptions
{
    ///<summary>
/// Base exception for domain/business logic errors
/// </summary>
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message)
        {
        }

        public DomainException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
