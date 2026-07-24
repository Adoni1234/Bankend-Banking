using ArtemisBanking.Core.Application.Dtos.SavingAccount;
using ArtemisBanking.Core.Application.Dtos.User;

namespace ArtemisBanking.Core.Application.Dtos.Beneficiary;

public class BeneficiaryDto
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required string SavingAccountId { get; set; }
    
    public SavingAccountDto? SavingAccount { get; set; }
    public UserDto? User { get; set; }
}