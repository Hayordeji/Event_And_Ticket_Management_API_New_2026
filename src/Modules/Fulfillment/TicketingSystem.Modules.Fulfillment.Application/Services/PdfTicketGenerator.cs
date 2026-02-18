using Microsoft.Extensions.Logging;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Fulfillment.Domain.Entitites;

namespace TicketingSystem.Modules.Fulfillment.Application.Services
{
    /// <summary>
    /// PDF ticket generator service
    /// Current implementation generates simple HTML-to-PDF (stub)
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
            QuestPDF.Settings.License = LicenseType.Community; 

        }

        public byte[] GenerateTicketPdf(Ticket ticket)
        {
            try
            {
                _logger.LogInformation(
             "Generating PDF for ticket {TicketNumber}",
             ticket.TicketNumber);

                var qrCodeBytes = _qrCodeGenerator.GenerateQrCodeImage(ticket.QrCodeData, 10);

                var pdfBytes = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A5);
                        page.Margin(1, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontFamily("Arial"));

                        page.Content().Column(column =>
                        {
                            column.Spacing(10);

                            // Header
                            column.Item()
                                .Background("#1a1a2e")
                                .Padding(15)
                                .AlignCenter()
                                .Text(ticket.EventName)
                                .FontSize(20)
                                .Bold()
                                .FontColor("#ffffff");

                            // Ticket Number
                            column.Item()
                                .AlignCenter()
                                .Text($"Ticket #{ticket.TicketNumber}")
                                .FontSize(11)
                                .FontColor("#666666");

                            // Divider
                            column.Item().LineHorizontal(1).LineColor("#dddddd");

                            // Event Details
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Column(left =>
                                {
                                    left.Spacing(5);
                                    left.Item().Text("EVENT DETAILS").FontSize(9).Bold().FontColor("#999999");
                                    left.Item().Text(ticket.EventName).Bold().FontSize(13);
                                    left.Item().Text($"📅 {ticket.EventStartDate:dddd, MMMM dd, yyyy}").FontSize(11);
                                    left.Item().Text($"⏰ {ticket.EventStartDate:hh:mm tt} - {ticket.EventEndDate:hh:mm tt}").FontSize(11);
                                    left.Item().Text($"📍 {ticket.VenueName}").FontSize(11);
                                    left.Item().Text($"    {ticket.VenueAddress}, {ticket.VenueCity}").FontSize(10).FontColor("#666666");
                                });
                            });

                            // Divider
                            column.Item().LineHorizontal(1).LineColor("#dddddd");

                            // Ticket Info + QR Code side by side
                            column.Item().Row(row =>
                            {
                                // Left: Ticket Details
                                row.RelativeItem().Column(left =>
                                {
                                    left.Spacing(5);
                                    left.Item().Text("TICKET INFO").FontSize(9).Bold().FontColor("#999999");
                                    left.Item().Text(ticket.TicketTypeName).Bold().FontSize(13);
                                    left.Item().Text($"👤 {ticket.CustomerFirstName} {ticket.CustomerLastName}").FontSize(11);
                                    left.Item().Text($"✉️  {ticket.CustomerEmail}").FontSize(10).FontColor("#666666");
                                    left.Item().Text($"💳 {ticket.Currency} {ticket.PricePaid:N2}").FontSize(11).Bold();
                                    left.Item().Text($"Order: {ticket.OrderNumber}").FontSize(9).FontColor("#999999");
                                });

                                // Right: QR Code
                                row.ConstantItem(120).Column(right =>
                                {
                                    right.Item().AlignCenter().Text("SCAN AT ENTRANCE").FontSize(8).Bold().FontColor("#999999");
                                    right.Item()
                                        .Width(110)
                                        .Height(110)
                                        .Image(qrCodeBytes);
                                    right.Item().AlignCenter().Text(ticket.Barcode).FontSize(9).FontFamily("Courier New").Bold();
                                });
                            });

                            // Divider
                            column.Item().LineHorizontal(1).LineColor("#dddddd");

                            // Footer
                            column.Item()
                                .AlignCenter()
                                .Text("Present this ticket at the entrance. Do not share.")
                                .FontSize(9)
                                .FontColor("#999999");
                        });
                    });
                }).GeneratePdf();

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
                _logger.LogInformation("Generating PDF for {TicketCount} tickets", tickets.Count);

                var pdfBytes = Document.Create(container =>
                {
                    foreach (var ticket in tickets)
                    {
                        var qrCodeBytes = _qrCodeGenerator.GenerateQrCodeImage(ticket.QrCodeData, 10);

                        container.Page(page =>
                        {
                            page.Size(PageSizes.A5);
                            page.Margin(1, Unit.Centimetre);
                            page.DefaultTextStyle(x => x.FontFamily("Arial"));

                            page.Content().Column(column =>
                            {
                                column.Spacing(10);

                                column.Item()
                                    .Background("#1a1a2e")
                                    .Padding(15)
                                    .AlignCenter()
                                    .Text(ticket.EventName)
                                    .FontSize(20)
                                    .Bold()
                                    .FontColor("#ffffff");

                                column.Item()
                                    .AlignCenter()
                                    .Text($"Ticket #{ticket.TicketNumber}")
                                    .FontSize(11)
                                    .FontColor("#666666");

                                column.Item().LineHorizontal(1).LineColor("#dddddd");

                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().Column(left =>
                                    {
                                        left.Spacing(5);
                                        left.Item().Text("EVENT DETAILS").FontSize(9).Bold().FontColor("#999999");
                                        left.Item().Text(ticket.EventName).Bold().FontSize(13);
                                        left.Item().Text($"📅 {ticket.EventStartDate:dddd, MMMM dd, yyyy}").FontSize(11);
                                        left.Item().Text($"⏰ {ticket.EventStartDate:hh:mm tt} - {ticket.EventEndDate:hh:mm tt}").FontSize(11);
                                        left.Item().Text($"📍 {ticket.VenueName}").FontSize(11);
                                        left.Item().Text($"    {ticket.VenueAddress}, {ticket.VenueCity}").FontSize(10).FontColor("#666666");
                                    });
                                });

                                column.Item().LineHorizontal(1).LineColor("#dddddd");

                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().Column(left =>
                                    {
                                        left.Spacing(5);
                                        left.Item().Text("TICKET INFO").FontSize(9).Bold().FontColor("#999999");
                                        left.Item().Text(ticket.TicketTypeName).Bold().FontSize(13);
                                        left.Item().Text($"👤 {ticket.CustomerFirstName} {ticket.CustomerLastName}").FontSize(11);
                                        left.Item().Text($"✉️  {ticket.CustomerEmail}").FontSize(10).FontColor("#666666");
                                        left.Item().Text($"💳 {ticket.Currency} {ticket.PricePaid:N2}").FontSize(11).Bold();
                                        left.Item().Text($"Order: {ticket.OrderNumber}").FontSize(9).FontColor("#999999");
                                    });

                                    row.ConstantItem(120).Column(right =>
                                    {
                                        right.Item().AlignCenter().Text("SCAN AT ENTRANCE").FontSize(8).Bold().FontColor("#999999");
                                        right.Item().Width(110).Height(110).Image(qrCodeBytes);
                                        right.Item().AlignCenter().Text(ticket.Barcode).FontSize(9).FontFamily("Courier New").Bold();
                                    });
                                });

                                column.Item().LineHorizontal(1).LineColor("#dddddd");

                                column.Item()
                                    .AlignCenter()
                                    .Text("Present this ticket at the entrance. Do not share.")
                                    .FontSize(9)
                                    .FontColor("#999999");
                            });
                        });
                    }
                }).GeneratePdf();

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
