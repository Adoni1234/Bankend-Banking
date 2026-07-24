using ArtemisBanking.Core.Application.Dtos.SavingAccount;
using ArtemisBanking.Core.Application.ViewModels.SavingAccount;
using ArtemisBanking.Core.Domain.Common.Enums;

namespace ArtemisBanking.Core.Application.ViewModels.Transaction;

// Modificado para que sea simetrico con la clase entidad
public class TransactionViewModel
{
    public required int Id { get; set; }
    public required decimal Amount { get; set; }
    public required string AccountNumber { get; set; }
    public required string CreatedById { get; set; }
    public required TransactionType Type { get; set; }
    public required string Beneficiary  { get; set; } // En este contexto, es el destino de la transaccion
    public required string Origin  { get; set; }
    public required DateTime Date { get; set; }
    public required TransactionStatus  Status { get; set; }
    public required TransactionSubType SubType { get; set; }
    public SavingAccountViewModel? SavingAccount { get; set; }
}