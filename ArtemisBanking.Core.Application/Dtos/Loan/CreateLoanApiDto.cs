namespace ArtemisBanking.Core.Application.Dtos.Loan;

public class CreateLoanApiDto
{
    public required string ClientId { get; set; }
    public required decimal Amount { get; set; }
    public required int TermMonths { get; set; } // Plazos en meses
    public required decimal AnualRate { get; set; }
}