using ArtemisBanking.Core.Application.Dtos.SavingAccount;
using ArtemisBanking.Core.Application.Dtos.User;
using ArtemisBanking.Core.Application.ViewModels.SavingAccount;
using ArtemisBanking.Core.Application.ViewModels.User;

namespace ArtemisBanking.Core.Application.ViewModels.Beneficiary;

public class BeneficiaryViewModel
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required string SavingAccountId { get; set; }
    
    public SavingAccountViewModel? SavingAccount { get; set; }
    public UserViewModel? User { get; set; }
}