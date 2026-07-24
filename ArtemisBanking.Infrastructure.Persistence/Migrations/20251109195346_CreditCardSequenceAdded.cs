using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtemisBanking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreditCardSequenceAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "SeqCreditCardsID",
                maxValue: 999999999L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "SeqCreditCardsID");
        }
    }
}
