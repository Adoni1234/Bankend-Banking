namespace ArtemisBanking.Core.Application.Dtos.Transaction;

public class TransactionToMainAccountDueCanceledAccountDto
{
    public string CanceledAccountNumber { get; set; } = string.Empty;
    public string AdminWhoCanceled { get; set; } = string.Empty;
}