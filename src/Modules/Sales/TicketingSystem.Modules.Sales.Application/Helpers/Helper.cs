using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Sales.Application.Helpers
{
    public static class Helper
    {
        /// <summary>
        /// Masks CAC number for logging (shows only last 3 digits)
        /// Example: "RC-1234567" -> "****567"
        /// </summary>
        public static string MaskRefNumber(string refNumber)
        {
            if (string.IsNullOrWhiteSpace(refNumber))
                return "****";

            return $"****{refNumber.Substring(refNumber.Length - 3)}";
        }

    }
}
