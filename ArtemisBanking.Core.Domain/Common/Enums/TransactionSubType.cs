namespace ArtemisBanking.Core.Domain.Common.Enums;

public enum TransactionSubType
{
    // Para transacciones de cuenta
    ExpressTransfer,
    CreditCardPayment, 
    LoanPayment,
    BeneficiaryTransfer,
    CashAdvance,
    AccountTransfer,
    
    // Para cajero
    Deposit, // tambien se usa en el desembolso de prestamos. El origen es el prestamo
    Withdrawal, // Retiro de dinero
    ThirdPartyTransfer // de cliente a cliente
}