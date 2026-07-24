namespace ArtemisBanking.Core.Application.Dtos.Transfer;

public class TransferDto
{
    public string FromAccountNumber { get; set; } = string.Empty;
    public string ToAccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
