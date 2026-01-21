using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TicketingSystem.SharedKernel.ApiResponses
{
    ///<summary>
    /// Standard API response wrapper
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates if the request was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Response data (null if error)
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Error message (null if success)
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Detailed error information (for debugging)
        /// </summary>
        public object? ErrorDetails { get; set; }

        /// <summary>
        /// Timestamp of the response
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Request trace ID for debugging
        /// </summary>
        public string? TraceId { get; set; }

        /// <summary>
        /// Create a successful response
        /// </summary>
        public static ApiResponse<T> SuccessResponse(T data, string? traceId = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                TraceId = traceId
            };
        }

        /// <summary>
        /// Create a error response
        /// </summary>
        public static ApiResponse<T> ErrorResponse(string error, string? traceId = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Error = error,
                TraceId = traceId
            };
        }
    }

    /// <summary>
    /// API response without data
    /// </summary>
    public class ApiResponse : ApiResponse<object>
    {
      
        public static ApiResponse SuccessResponse(string? traceId = null)
        {
            return new ApiResponse
            {
                Success = true,
                TraceId = traceId
            };
        }

        public new static ApiResponse ErrorResponse(string error, object? errorDetails = null, string? traceId = null)
        {
            return new ApiResponse
            {
                Success = false,
                Error = error,
                ErrorDetails = errorDetails,
                TraceId = traceId
            };
        }
    }

}
