using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderSystem.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailTypeToEmailNotificationHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailType",
                table: "EmailNotificationHistories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Legacy"
            );

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotificationHistories_OrderId_EmailType_Status",
                table: "EmailNotificationHistories",
                columns: new[] { "OrderId", "EmailType", "Status" }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailNotificationHistories_OrderId_EmailType_Status",
                table: "EmailNotificationHistories"
            );

            migrationBuilder.DropColumn(name: "EmailType", table: "EmailNotificationHistories");
        }
    }
}
