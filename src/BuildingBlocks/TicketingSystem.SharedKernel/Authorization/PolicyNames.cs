using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.SharedKernel.Authorization
{
    public static class PolicyNames
    {
        public const string RequireAdmin = "RequireAdmin";
        public const string RequireHost = "RequireHost";
        public const string RequireCustomer = "RequireCustomer";
        public const string RequireScanner = "RequireScanner";
        public const string RequireHostOrScanner = "RequireHostOrScanner";
    }
}
