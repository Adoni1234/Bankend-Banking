namespace ArtemisBanking.Core.Application.Dtos.Loan;

public class AssignLoanDto
{
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int DurationMonths { get; set; }
    public decimal AnnualInterestRate { get; set; }
    public string AdminUserId { get; set; } = string.Empty;
    public bool ConfirmHighRisk { get; set; }
}