using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderSystem.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderFulfillmentStatusFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "OrderStatusHistories",
                type: "nvarchar(max)",
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAtUtc",
                table: "Orders",
                type: "datetime2",
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "PreparationStartedAtUtc",
                table: "Orders",
                type: "datetime2",
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadyForShipmentAtUtc",
                table: "Orders",
                type: "datetime2",
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnedAtUtc",
                table: "Orders",
                type: "datetime2",
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "ShippedAtUtc",
                table: "Orders",
                type: "datetime2",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "TrackingNumber",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Note", table: "OrderStatusHistories");

            migrationBuilder.DropColumn(name: "DeliveredAtUtc", table: "Orders");

            migrationBuilder.DropColumn(name: "PreparationStartedAtUtc", table: "Orders");

            migrationBuilder.DropColumn(name: "ReadyForShipmentAtUtc", table: "Orders");

            migrationBuilder.DropColumn(name: "ReturnedAtUtc", table: "Orders");

            migrationBuilder.DropColumn(name: "ShippedAtUtc", table: "Orders");

            migrationBuilder.DropColumn(name: "TrackingNumber", table: "Orders");
        }
    }
}
