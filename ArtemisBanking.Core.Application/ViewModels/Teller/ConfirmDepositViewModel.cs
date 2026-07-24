using ArtemisBanking.Core.Application.Dtos.SavingAccount;

namespace ArtemisBanking.Core.Application.ViewModels.Teller;

public class ConfirmDepositViewModel
{
    public DepositViewModel Deposit { get; set; }
    public string ClientName { get; set; }
}