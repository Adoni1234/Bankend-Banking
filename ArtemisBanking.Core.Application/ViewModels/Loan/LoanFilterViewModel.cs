namespace ArtemisBanking.Core.Application.ViewModels.Loan;

public class LoanFilterViewModel
{
    public string? IdentityCardNumber { get; set; }
    public bool? IsCompleted { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}