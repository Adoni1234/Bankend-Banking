namespace ArtemisBanking.Core.Application.Dtos.CreditCard;

public class CreditCardFiltersDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? IdentityCardNumber{ get; set; }
    public bool? IsActive { get; set; }
}