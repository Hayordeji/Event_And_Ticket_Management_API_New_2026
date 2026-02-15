using Microsoft.Extensions.Logging;
using QRCoder;

namespace TicketingSystem.Modules.Fulfillment.Application.Services
{
    /// <summary>
    /// QR code generator service using QRCoder library
    /// NuGet Package Required: QRCoder (>= 1.4.3)
    /// </summary>
    public class QrCodeGenerator : IQrCodeGenerator
    {
        private readonly ILogger<QrCodeGenerator> _logger;

        public QrCodeGenerator(ILogger<QrCodeGenerator> logger)
        {
            _logger = logger;
        }

        public byte[] GenerateQrCodeImage(string data, int pixelsPerModule = 20)
        {
            try
            {
                _logger.LogDebug("Generating QR code image. DataLength={Length}, PixelsPerModule={Pixels}",
                    data.Length, pixelsPerModule);

                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);

                var imageBytes = qrCode.GetGraphic(pixelsPerModule);

                _logger.LogDebug("QR code image generated successfully. Size={Size} bytes", imageBytes.Length);

                return imageBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code image");
                throw;
            }
        }

        public string GenerateQrCodeBase64(string data, int pixelsPerModule = 20)
        {
            try
            {
                _logger.LogDebug("Generating QR code as Base64. DataLength={Length}", data.Length);

                var imageBytes = GenerateQrCodeImage(data, pixelsPerModule);
                var base64 = Convert.ToBase64String(imageBytes);

                _logger.LogDebug("QR code Base64 generated successfully. Length={Length}", base64.Length);

                return base64;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code Base64");
                throw;
            }
        }
    }
}
