namespace ArtemisBanking.Core.Application.ViewModels.SavingAccount;

public class SavingAccountIndexViewModel
{
    public required PaginatedData<SavingAccountViewModel> Data { get; set; }
    public required SavingAccountFiltersViewModel Filter { get; set; }
}