namespace ArtemisBanking.Core.Application.Dtos.LoanInstallment;

public class AmortizationTableDto
{
    public required string LoanId { get; set; }
    public required decimal LoanAmount { get; set; }
    public required decimal AnnualInterestRate { get; set; }
    public required int TotalInstallments { get; set; }
    public List<LoanInstallmentDto>? Installments { get; set; } 
}