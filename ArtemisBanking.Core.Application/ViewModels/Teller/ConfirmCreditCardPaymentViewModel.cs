namespace ArtemisBanking.Core.Application.ViewModels.Teller;

public class ConfirmCreditCardPaymentViewModel
{
    public CreditCardPaymentViewModel Payment { get; set; } = new();
    public string CardOwnerName { get; set; } = string.Empty;
}
