using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChallengeBank.Clients.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandClientsAndTransfers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressCity",
                schema: "clients",
                table: "Clients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressPostalCode",
                schema: "clients",
                table: "Clients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressState",
                schema: "clients",
                table: "Clients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressStreet",
                schema: "clients",
                table: "Clients",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                schema: "clients",
                table: "Clients",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAgency",
                schema: "clients",
                table: "Clients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressCity",
                schema: "clients",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "AddressPostalCode",
                schema: "clients",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "AddressState",
                schema: "clients",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "AddressStreet",
                schema: "clients",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                schema: "clients",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "BankAgency",
                schema: "clients",
                table: "Clients");
        }
    }
}
