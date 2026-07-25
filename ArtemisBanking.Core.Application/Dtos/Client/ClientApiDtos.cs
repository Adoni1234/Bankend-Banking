using ArtemisBanking.Core.Domain.Common.Enums;

namespace ArtemisBanking.Core.Application.Dtos.Client;

// DTOs "planos" para los endpoints del área Client de la API, pensados para
// que Flutter los consuma directo sin arrastrar los grafos de navegación
// (SavingAccountDto -> TransactionDto -> SavingAccountDto -> ...) que tienen
// los Dtos de dominio y que rompen la serialización JSON por ciclos.

public class ClientAccountApiDto
{
    public required string AccountNumber { get; set; }
    public required decimal Balance { get; set; }
    public required bool IsPrincipalAccount { get; set; }
    public required bool IsActive { get; set; }
    public required DateTime CreatedAt { get; set; }
}

public class ClientTransactionApiDto
{
    public required int Id { get; set; }
    public required decimal Amount { get; set; }
    public required string AccountNumber { get; set; }
    public required TransactionType Type { get; set; }
    public required TransactionSubType SubType { get; set; }
    public required TransactionStatus Status { get; set; }
    public required string Beneficiary { get; set; }
    public required string Origin { get; set; }
    public required DateTime Date { get; set; }
}

public class ClientHomeApiDto
{
    public required ClientAccountApiDto MainAccount { get; set; }
    public required List<ClientTransactionApiDto> RecentTransactions { get; set; }
}

public class ClientBeneficiaryApiDto
{
    public required int Id { get; set; }
    public required string AccountNumber { get; set; }
    public string? OwnerName { get; set; }
}

public class CreateBeneficiaryApiDto
{
    public required string AccountNumber { get; set; }
}

public class ClientCreditCardApiDto
{
    public required string CardNumber { get; set; }
    public required decimal CreditLimit { get; set; }
    public required decimal Balance { get; set; }
    public required bool IsActive { get; set; }
}

public class ClientLoanApiDto
{
    public required string Id { get; set; }
    public required decimal Amount { get; set; }
    public required int TermMonths { get; set; }
    public required decimal AnualRate { get; set; }
    public required bool Completed { get; set; }
    public required bool IsDue { get; set; }
}

public class ClientPaymentOptionsApiDto
{
    public required List<ClientAccountApiDto> Accounts { get; set; }
    public required List<ClientCreditCardApiDto> CreditCards { get; set; }
    public required List<ClientLoanApiDto> Loans { get; set; }
}

public class PayCreditCardApiDto
{
    public required string SourceAccountNumber { get; set; }
    public required string CreditCardNumber { get; set; }
    public required decimal Amount { get; set; }
}

public class PayLoanApiDto
{
    public required string SourceAccountNumber { get; set; }
    public required string LoanNumber { get; set; }
    public required decimal Amount { get; set; }
}