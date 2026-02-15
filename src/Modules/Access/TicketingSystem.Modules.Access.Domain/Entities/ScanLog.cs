using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Access.Domain.Enums;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Access.Domain.Entities
{
    public class ScanLog : AggregateRoot
    {
        public Guid TicketId { get; private set; }
        public string TicketNumber { get; private set; } = string.Empty;
        public Guid EventId { get; private set; }
        public Guid ScannedBy { get; private set; }
        public string DeviceId { get; private set; } = string.Empty;
        public string GateLocation { get; private set; } = string.Empty;
        public ScanResult Result { get; private set; }
        public DenialReason? DenialReason { get; private set; }
        public DateTime ScannedAt { get; private set; }

        private ScanLog() { }

        public static ScanLog RecordAllowed(
            Guid ticketId,
            string ticketNumber,
            Guid eventId,
            Guid scannedBy,
            string deviceId,
            string gateLocation)
        {
            var log = new ScanLog
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                TicketNumber = ticketNumber,
                EventId = eventId,
                ScannedBy = scannedBy,
                DeviceId = deviceId,
                GateLocation = gateLocation,
                Result = ScanResult.Allowed,
                DenialReason = null,
                ScannedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            

            return log;
        }

        public static ScanLog RecordDenied(
            Guid ticketId,
            string ticketNumber,
            Guid eventId,
            Guid scannedBy,
            string deviceId,
            string gateLocation,
            DenialReason reason)
        {
            var log = new ScanLog
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                TicketNumber = ticketNumber,
                EventId = eventId,
                ScannedBy = scannedBy,
                DeviceId = deviceId,
                GateLocation = gateLocation,
                Result = ScanResult.Denied,
                DenialReason = reason,
                ScannedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

           

            return log;
        }
    }
}
