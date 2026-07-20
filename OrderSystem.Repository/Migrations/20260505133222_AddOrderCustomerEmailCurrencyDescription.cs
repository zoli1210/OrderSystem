using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderSystem.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderCustomerEmailCurrencyDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Orders",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                table: "Orders",
                type: "nvarchar(320)",
                maxLength: 320,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Currency", table: "Orders");

            migrationBuilder.DropColumn(name: "CustomerEmail", table: "Orders");

            migrationBuilder.DropColumn(name: "Description", table: "Orders");
        }
    }
}
