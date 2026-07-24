using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtemisBanking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SomeColumnsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SubType",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CapitalAmount",
                table: "LoanInstallments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InterestAmount",
                table: "LoanInstallments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "CommerceId",
                table: "CardTransactions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsCashAdvance",
                table: "CardTransactions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "SubType",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "CapitalAmount",
                table: "LoanInstallments");

            migrationBuilder.DropColumn(
                name: "InterestAmount",
                table: "LoanInstallments");

            migrationBuilder.DropColumn(
                name: "IsCashAdvance",
                table: "CardTransactions");

            migrationBuilder.AlterColumn<int>(
                name: "CommerceId",
                table: "CardTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
