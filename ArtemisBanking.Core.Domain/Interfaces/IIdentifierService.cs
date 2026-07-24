namespace ArtemisBanking.Core.Domain.Interfaces;

public interface IIdentifierService
{
    Task<string> GenerateIdentifier();
    Task<string> GenerateCreditCardNumber();
}