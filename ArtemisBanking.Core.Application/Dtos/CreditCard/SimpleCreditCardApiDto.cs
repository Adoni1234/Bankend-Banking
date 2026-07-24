namespace ArtemisBanking.Core.Application.Dtos.CreditCard;

public class SimpleCreditCardApiDto
{
    public required string CardNumber { get; set; }
    public required string Client { get; set; }
    public required string ExpirationDate { get; set; }
    public required decimal Limit { get; set; }
    public required decimal Balance { get; set; }
    public required string Status { get; set; }
}