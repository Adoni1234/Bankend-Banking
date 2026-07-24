using ArtemisBanking.Core.Application.Dtos.Transaction;
using ArtemisBanking.Core.Application.Dtos.User;
using ArtemisBanking.Core.Application.ViewModels.Transaction;
using ArtemisBanking.Core.Application.ViewModels.User;

namespace ArtemisBanking.Core.Application.ViewModels.SavingAccount;

// Modificado para que sea simetrico con la clase entidad
public class SavingAccountViewModel
{
    public string Id { get; set; } // Las cuentas y prestamos obtienen su ID de la secuencia global
    public required string ClientId { get; set; } // Es un UserId
    public required decimal Balance { get; set; } // Saldo de la cuenta
    public required DateTime CreatedAt { get; set; } = DateTime.Now;
    public required string AssignedByUserId { get; set; }
    public required bool IsPrincipalAccount { get; set; }
    public required bool IsActive { get; set; }
    
    public ICollection<TransactionViewModel> Transactions { get; set; } = [];
    
    public UserViewModel? AssignedByUser { get; set; }
    public UserViewModel? Client { get; set; }
}