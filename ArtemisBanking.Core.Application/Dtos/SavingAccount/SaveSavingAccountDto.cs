namespace ArtemisBanking.Core.Application.Dtos.SavingAccount;

public class SaveSavingAccountDto
{
    public string UserId { get; set; } = string.Empty;
    public decimal Balance { get; set; } 
    public bool IsMainAccount { get; set; }
    public string AdminUserId { get; set; } = string.Empty;
}