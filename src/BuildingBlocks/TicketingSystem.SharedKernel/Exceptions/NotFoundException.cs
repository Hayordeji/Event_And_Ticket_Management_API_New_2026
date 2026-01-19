using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.SharedKernel.Exceptions
{
     ///<summary>
/// Exception thrown when a requested resource is not found
/// </summary>
    public class NotFoundException : DomainException
    {
        public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.")
        {
        }

        public NotFoundException(string message) : base(message)
        {
        }
    }
}
