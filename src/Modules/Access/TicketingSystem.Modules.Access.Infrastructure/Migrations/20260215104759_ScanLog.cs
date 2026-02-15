using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketingSystem.Modules.Access.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ScanLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "access");

            migrationBuilder.CreateTable(
                name: "ScanLogs",
                schema: "access",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScannedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GateLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Result = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DenialReason = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ScannedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScanLogs_EventId",
                schema: "access",
                table: "ScanLogs",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_ScanLogs_Result",
                schema: "access",
                table: "ScanLogs",
                column: "Result");

            migrationBuilder.CreateIndex(
                name: "IX_ScanLogs_ScannedAt",
                schema: "access",
                table: "ScanLogs",
                column: "ScannedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ScanLogs_TicketId",
                schema: "access",
                table: "ScanLogs",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_ScanLogs_TicketNumber",
                schema: "access",
                table: "ScanLogs",
                column: "TicketNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScanLogs",
                schema: "access");
        }
    }
}
