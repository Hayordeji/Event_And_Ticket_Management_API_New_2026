using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.SharedKernel.Exceptions
{
    /// <summary>
/// Exception thrown when validation fails
/// </summary>
    public class ValidationException : DomainException
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(IDictionary<string, string[]> errors)
            : base("One or more validation errors occurred.")
        {
            Errors = errors;
        }

        public ValidationException(string field, string error)
            : base("Validation error occurred.")
        {
            Errors = new Dictionary<string, string[]>
        {
            { field, new[] { error } }
        };
        }
    }
}
