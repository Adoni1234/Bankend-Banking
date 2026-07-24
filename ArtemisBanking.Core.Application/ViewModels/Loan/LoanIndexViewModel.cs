namespace ArtemisBanking.Core.Application.ViewModels.Loan;

public class LoanIndexViewModel
{
    public required PaginatedData<LoanSummaryViewModel> Data { get; set; }
    public required LoanFilterViewModel Filter { get; set; }
}