using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderSystem.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueSentEmailPerOrderAndType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailNotificationHistories_OrderId_EmailType_Status",
                table: "EmailNotificationHistories"
            );

            migrationBuilder.Sql(
                """
                ;WITH DuplicateSentEmails AS
                (
                    SELECT
                        Id,
                        ROW_NUMBER() OVER (
                            PARTITION BY OrderId, EmailType
                            ORDER BY 
                                CASE WHEN SentAtUtc IS NULL THEN 1 ELSE 0 END,
                                SentAtUtc DESC,
                                CreatedAtUtc DESC,
                                Id DESC
                        ) AS RowNumber
                    FROM EmailNotificationHistories
                    WHERE Status = 2
                )
                DELETE FROM DuplicateSentEmails
                WHERE RowNumber > 1;
                """
            );

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotificationHistories_OrderId_EmailType",
                table: "EmailNotificationHistories",
                columns: new[] { "OrderId", "EmailType" },
                unique: true,
                filter: "[Status] = 2"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailNotificationHistories_OrderId_EmailType",
                table: "EmailNotificationHistories"
            );

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotificationHistories_OrderId_EmailType_Status",
                table: "EmailNotificationHistories",
                columns: new[] { "OrderId", "EmailType", "Status" }
            );
        }
    }
}
