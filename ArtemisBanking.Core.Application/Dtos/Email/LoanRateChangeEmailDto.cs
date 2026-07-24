namespace ArtemisBanking.Core.Application.Dtos.Email;

public class LoanRateChangeEmailDto
{
    public string To { get; set; } = string.Empty; 
    public string ClientName { get; set; } = string.Empty;
    public string LoanNumber { get; set; } = string.Empty;
    public decimal NewInterestRate { get; set; }
    public decimal NewMonthlyPayment { get; set; }
}