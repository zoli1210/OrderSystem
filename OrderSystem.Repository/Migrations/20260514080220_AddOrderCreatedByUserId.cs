using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderSystem.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderCreatedByUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Orders",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: ""
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CreatedByUserId", table: "Orders");
        }
    }
}
