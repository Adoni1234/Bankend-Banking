namespace ArtemisBanking.Core.Application.ViewModels.SavingAccount;

public class SavingAccountFiltersViewModel
{
    public string? IdentityCardNumber { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}