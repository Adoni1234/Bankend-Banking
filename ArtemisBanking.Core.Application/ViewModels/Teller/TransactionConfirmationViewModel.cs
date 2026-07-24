using ArtemisBanking.Core.Application.Dtos.Transaction;

namespace ArtemisBanking.Core.Application.ViewModels.Teller;

public class TransactionConfirmationViewModel
{
    public required TransactionDto Transaction { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string? AccountNumber { get; set; }
    public string? DestinationAccountNumber { get; set; }
    public string? CreditCardNumber { get; set; }
    public string? LoanNumber { get; set; }
}

