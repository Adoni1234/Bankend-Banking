namespace ArtemisBanking.Core.Application.ViewModels.CreditCard;

public class CreditCardIndexViewModel
{
    public required PaginatedData<CreditCardViewModel> Data { get; set; }
    public required CreditCardFilterViewModel Filter { get; set; }
}