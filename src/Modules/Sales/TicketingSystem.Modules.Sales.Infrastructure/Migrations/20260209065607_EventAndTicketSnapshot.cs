using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketingSystem.Modules.Sales.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EventAndTicketSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventStartDate",
                schema: "sales",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "VenueCity",
                schema: "sales",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "VenueName",
                schema: "sales",
                table: "OrderItems");

            migrationBuilder.AddColumn<string>(
                name: "EventDescription",
                schema: "sales",
                table: "Orders",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EventEndDate",
                schema: "sales",
                table: "Orders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EventName",
                schema: "sales",
                table: "Orders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EventStartDate",
                schema: "sales",
                table: "Orders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "VenueAddress",
                schema: "sales",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VenueCity",
                schema: "sales",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VenueName",
                schema: "sales",
                table: "Orders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "TicketTypeName",
                schema: "sales",
                table: "OrderItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "TicketTypeDescription",
                schema: "sales",
                table: "OrderItems",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventDescription",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "EventEndDate",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "EventName",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "EventStartDate",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "VenueAddress",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "VenueCity",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "VenueName",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TicketTypeDescription",
                schema: "sales",
                table: "OrderItems");

            migrationBuilder.AlterColumn<string>(
                name: "TicketTypeName",
                schema: "sales",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<DateTime>(
                name: "EventStartDate",
                schema: "sales",
                table: "OrderItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "VenueCity",
                schema: "sales",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VenueName",
                schema: "sales",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
