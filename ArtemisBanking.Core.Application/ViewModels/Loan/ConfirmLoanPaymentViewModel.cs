using ArtemisBanking.Core.Application.ViewModels.Teller;

namespace ArtemisBanking.Core.Application.ViewModels.Loan;

public class ConfirmLoanPaymentViewModel
{
    public LoanPaymentViewModel Payment { get; set; } = new();
    public string LoanOwnerName { get; set; } = string.Empty;
}
