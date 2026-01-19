using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.SharedKernel.ApiResponses
{
   ///<summary>
/// Response for validation errors
/// </summary>
    public class ValidationErrorResponse
    {
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class ValidationErrorsResponse
    {
        public List<ValidationErrorResponse> Errors { get; set; } = new();
    }
}
