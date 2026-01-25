using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketingSystem.Modules.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.CreateTable(
                name: "Events",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    VenueName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VenueAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    VenueCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VenueState = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VenueCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VenuePostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    VenueLatitude = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: true),
                    VenueLongitude = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsCancelled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HasSnapshot = table.Column<bool>(type: "bit", nullable: false),
                    CurrentSnapshotVersion = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventSnapshots",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SnapshotVersion = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    VenueName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VenueAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    VenueCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VenueState = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VenueCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VenuePostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    VenueLatitude = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: true),
                    VenueLongitude = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SnapshotCreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventSnapshots_Events_EventId",
                        column: x => x.EventId,
                        principalSchema: "catalog",
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventSnapshots_Events_EventId1",
                        column: x => x.EventId1,
                        principalSchema: "catalog",
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketTypes",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PriceAmount = table.Column<decimal>(type: "decimal(19,4)", nullable: false),
                    PriceCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TotalCapacity = table.Column<int>(type: "int", nullable: false),
                    SoldCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ReservedCount = table.Column<int>(type: "int", nullable: false),
                    SaleStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SaleEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MinPurchaseQuantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MaxPurchaseQuantity = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EventId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketTypes_Events_EventId",
                        column: x => x.EventId,
                        principalSchema: "catalog",
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketTypes_Events_EventId1",
                        column: x => x.EventId1,
                        principalSchema: "catalog",
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_HostId",
                schema: "catalog",
                table: "Events",
                column: "HostId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_IsPublished",
                schema: "catalog",
                table: "Events",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_Events_IsPublished_StartDate",
                schema: "catalog",
                table: "Events",
                columns: new[] { "IsPublished", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_StartDate",
                schema: "catalog",
                table: "Events",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_EventSnapshots_EventId",
                schema: "catalog",
                table: "EventSnapshots",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventSnapshots_EventId1",
                schema: "catalog",
                table: "EventSnapshots",
                column: "EventId1");

            migrationBuilder.CreateIndex(
                name: "IX_EventSnapshots_SnapshotCreatedAt",
                schema: "catalog",
                table: "EventSnapshots",
                column: "SnapshotCreatedAt");

            migrationBuilder.CreateIndex(
                name: "UQ_EventSnapshots_EventId_Version",
                schema: "catalog",
                table: "EventSnapshots",
                columns: new[] { "EventId", "SnapshotVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketTypes_EventId",
                schema: "catalog",
                table: "TicketTypes",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTypes_EventId1",
                schema: "catalog",
                table: "TicketTypes",
                column: "EventId1");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTypes_IsActive",
                schema: "catalog",
                table: "TicketTypes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTypes_SalesWindow",
                schema: "catalog",
                table: "TicketTypes",
                columns: new[] { "SaleStartDate", "SaleEndDate" });

            migrationBuilder.CreateIndex(
                name: "UQ_TicketTypes_EventId_Name",
                schema: "catalog",
                table: "TicketTypes",
                columns: new[] { "EventId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventSnapshots",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "TicketTypes",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "Events",
                schema: "catalog");
        }
    }
}
