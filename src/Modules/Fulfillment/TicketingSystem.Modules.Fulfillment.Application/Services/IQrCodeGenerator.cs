using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Fulfillment.Application.Services
{
    public interface IQrCodeGenerator
    {
        /// <summary>
        /// Generates QR code as PNG byte array
        /// </summary>
        byte[] GenerateQrCodeImage(string data, int pixelsPerModule = 20);

        /// <summary>
        /// Generates QR code as Base64 string (for embedding in HTML/JSON)
        /// </summary>
        string GenerateQrCodeBase64(string data, int pixelsPerModule = 20);
    }
}
