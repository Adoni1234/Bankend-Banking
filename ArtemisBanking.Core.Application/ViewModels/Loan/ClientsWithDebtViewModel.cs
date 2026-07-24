using ArtemisBanking.Core.Application.Dtos.User;
using ArtemisBanking.Core.Application.ViewModels.User;

namespace ArtemisBanking.Core.Application.ViewModels.Loan;

public class ClientsWithDebtViewModel
{
    public required UserViewModel Client { get; set; }
    public required decimal Debt { get; set; }
}