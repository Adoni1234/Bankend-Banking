namespace ArtemisBanking.Core.Application.Enums;

public enum EmailType
{
    LoanApproved,
    LoanRateUpdated,
    LoanDisbursement,
    LoanPayment,
    
    CreditCardPayment,
    CashAdvance,
    CreditCardLimitUpdated,
    
    ExpressTransferSender,
    ExpressTransferReceiver,
    
    BeneficiaryTransferSender,
    BeneficiaryTransferReceiver,
    
    Deposit,
    Withdraw
}
