using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderSystem.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAtUtc",
                table: "Orders",
                type: "datetime2",
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Orders",
                type: "datetime2",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "Orders",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CancellationReason", table: "Orders");

            migrationBuilder.DropColumn(name: "CancelledAtUtc", table: "Orders");

            migrationBuilder.DropColumn(name: "UpdatedAtUtc", table: "Orders");

            migrationBuilder.DropColumn(name: "UpdatedByUserId", table: "Orders");
        }
    }
}
