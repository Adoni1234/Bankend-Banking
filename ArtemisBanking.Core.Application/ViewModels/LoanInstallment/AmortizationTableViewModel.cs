namespace ArtemisBanking.Core.Application.ViewModels.LoanInstallment;

public class AmortizationTableViewModel
{
    public required string LoanId { get; set; }
    public required decimal LoanAmount { get; set; }
    public required decimal AnnualInterestRate { get; set; }
    public required int TotalInstallments { get; set; }
    public List<LoanInstallmentViewModel>? Installments { get; set; } 
}