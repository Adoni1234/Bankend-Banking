namespace ArtemisBanking.Core.Application.Dtos.Email;

public class LoanApprovedEmailDto
{
    public string To { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public decimal ApprovedAmount { get; set; }
    public int DurationMonths { get; set; }
    public decimal AnnualInterestRate { get; set; }
    public decimal MonthlyPayment { get; set; }
}