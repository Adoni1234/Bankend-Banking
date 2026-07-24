namespace ArtemisBanking.Core.Application.Dtos.CreditCard;

public class CreateCreditCardApiDto
{
    public required string ClientId { get; set; }
    public required decimal Limit { get; set; }
}