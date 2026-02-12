using Microsoft.Extensions.Logging;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Fulfillment.Domain.Entitites;

namespace TicketingSystem.Modules.Fulfillment.Application.Services
{
    /// <summary>
    /// PDF ticket generator service
    /// TODO: Implement using QuestPDF library for production-quality PDFs
    /// Current implementation generates simple HTML-to-PDF (stub)
    /// NuGet Package: QuestPDF or SelectPdf or iTextSharp
    /// </summary>
    public class PdfTicketGenerator : IPdfTicketGenerator
    {
        private readonly IQrCodeGenerator _qrCodeGenerator;
        private readonly ILogger<PdfTicketGenerator> _logger;

        public PdfTicketGenerator(
            IQrCodeGenerator qrCodeGenerator,
            ILogger<PdfTicketGenerator> logger)
        {
            _qrCodeGenerator = qrCodeGenerator;
            _logger = logger;
        }

        public byte[] GenerateTicketPdf(Ticket ticket)
        {
            try
            {
                _logger.LogInformation(
                    "Generating PDF for ticket {TicketNumber}",
                    ticket.TicketNumber);

                var html = GenerateTicketHtml(ticket);

                // TODO: Convert HTML to PDF using a proper library
                // For now, returning HTML as bytes (this is a stub)
                // In production, use QuestPDF or SelectPdf
                var pdfBytes = Encoding.UTF8.GetBytes(html);

                _logger.LogInformation(
                    "PDF generated for ticket {TicketNumber}. Size={Size} bytes",
                    ticket.TicketNumber, pdfBytes.Length);

                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error generating PDF for ticket {TicketNumber}",
                    ticket.TicketNumber);
                throw;
            }
        }

        public byte[] GenerateTicketsPdf(List<Ticket> tickets)
        {
            try
            {
                _logger.LogInformation(
                    "Generating PDF for {TicketCount} tickets",
                    tickets.Count);

                var htmlBuilder = new StringBuilder();
                htmlBuilder.AppendLine("<html><head><style>");
                htmlBuilder.AppendLine(GetPdfStyles());
                htmlBuilder.AppendLine("</style></head><body>");

                foreach (var ticket in tickets)
                {
                    htmlBuilder.AppendLine(GenerateTicketHtml(ticket));
                    htmlBuilder.AppendLine("<div style='page-break-after: always;'></div>");
                }

                htmlBuilder.AppendLine("</body></html>");

                // TODO: Convert HTML to PDF
                var pdfBytes = Encoding.UTF8.GetBytes(htmlBuilder.ToString());

                _logger.LogInformation(
                    "PDF generated for {TicketCount} tickets. Size={Size} bytes",
                    tickets.Count, pdfBytes.Length);

                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error generating PDF for {TicketCount} tickets",
                    tickets.Count);
                throw;
            }
        }

        private string GenerateTicketHtml(Ticket ticket)
        {
            var qrCodeBase64 = _qrCodeGenerator.GenerateQrCodeBase64(ticket.QrCodeData, 10);

            return $@"
        <div class='ticket'>
            <div class='ticket-header'>
                <h1>{ticket.EventName}</h1>
                <p class='ticket-number'>Ticket #{ticket.TicketNumber}</p>
            </div>
            
            <div class='ticket-body'>
                <div class='event-details'>
                    <h2>Event Details</h2>
                    <p><strong>Date:</strong> {ticket.EventStartDate:dddd, MMMM dd, yyyy}</p>
                    <p><strong>Time:</strong> {ticket.EventStartDate:hh:mm tt} - {ticket.EventEndDate:hh:mm tt}</p>
                    <p><strong>Venue:</strong> {ticket.VenueName}</p>
                    <p><strong>Address:</strong> {ticket.VenueAddress}, {ticket.VenueCity}</p>
                </div>
                
                <div class='ticket-details'>
                    <h2>Ticket Information</h2>
                    <p><strong>Ticket Type:</strong> {ticket.TicketTypeName}</p>
                    <p><strong>Holder:</strong> {ticket.CustomerFirstName} {ticket.CustomerLastName}</p>
                    <p><strong>Price:</strong> {ticket.Currency} {ticket.PricePaid:N2}</p>
                    <p><strong>Order:</strong> {ticket.OrderNumber}</p>
                </div>
                
                <div class='qr-code'>
                    <h2>Scan at Entrance</h2>
                    <img src='data:image/png;base64,{qrCodeBase64}' alt='QR Code' />
                    <p class='barcode'>{ticket.Barcode}</p>
                </div>
            </div>
            
            <div class='ticket-footer'>
                <p>Please present this ticket at the entrance. Keep it safe and do not share.</p>
                <p>Status: <strong>{ticket.Status}</strong></p>
            </div>
        </div>";
        }

        private static string GetPdfStyles()
        {
            return @"
        body {
            font-family: Arial, sans-serif;
            margin: 20px;
        }
        .ticket {
            border: 2px solid #333;
            border-radius: 10px;
            padding: 20px;
            margin-bottom: 20px;
            background: white;
        }
        .ticket-header {
            text-align: center;
            border-bottom: 2px dashed #ccc;
            padding-bottom: 15px;
            margin-bottom: 20px;
        }
        .ticket-header h1 {
            margin: 0;
            color: #333;
        }
        .ticket-number {
            color: #666;
            font-size: 14px;
            margin-top: 5px;
        }
        .ticket-body {
            display: flex;
            flex-direction: column;
        }
        .event-details, .ticket-details {
            margin-bottom: 20px;
        }
        .event-details h2, .ticket-details h2 {
            font-size: 16px;
            color: #333;
            border-bottom: 1px solid #eee;
            padding-bottom: 5px;
        }
        .event-details p, .ticket-details p {
            margin: 5px 0;
            font-size: 14px;
        }
        .qr-code {
            text-align: center;
            margin-top: 20px;
        }
        .qr-code img {
            width: 200px;
            height: 200px;
        }
        .barcode {
            font-family: 'Courier New', monospace;
            font-size: 18px;
            font-weight: bold;
            margin-top: 10px;
        }
        .ticket-footer {
            text-align: center;
            border-top: 2px dashed #ccc;
            padding-top: 15px;
            margin-top: 20px;
            font-size: 12px;
            color: #666;
        }";
        }
    }
}
