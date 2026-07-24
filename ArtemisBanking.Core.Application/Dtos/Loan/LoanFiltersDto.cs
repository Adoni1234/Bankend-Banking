namespace ArtemisBanking.Core.Application.Dtos.Loan;

public class LoanFiltersDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? IdentityCardNumber{ get; set; }
    public bool? IsCompleted { get; set; }
}