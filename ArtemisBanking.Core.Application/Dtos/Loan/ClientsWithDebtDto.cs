using ArtemisBanking.Core.Application.Dtos.User;

namespace ArtemisBanking.Core.Application.Dtos.Loan;

public class ClientsWithDebtDto
{
    public required UserDto Client { get; set; }
    public required decimal Debt { get; set; }
}