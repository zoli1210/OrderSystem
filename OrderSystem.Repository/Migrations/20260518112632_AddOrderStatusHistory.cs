using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderSystem.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderStatusHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStatus = table.Column<int>(type: "int", nullable: false),
                    ToStatus = table.Column<int>(type: "int", nullable: false),
                    ChangedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedByUserId = table.Column<string>(
                        type: "nvarchar(450)",
                        maxLength: 450,
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatusHistories", x => x.Id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistories_ChangedAtUtc",
                table: "OrderStatusHistories",
                column: "ChangedAtUtc"
            );

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistories_OrderId",
                table: "OrderStatusHistories",
                column: "OrderId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "OrderStatusHistories");
        }
    }
}
