using ArtemisBanking.Core.Application.Dtos.CreditCard;
using ArtemisBanking.Core.Application.Dtos.Loan;
using ArtemisBanking.Core.Application.Dtos.SavingAccount;
using ArtemisBanking.Core.Application.Dtos.Transaction;
using ArtemisBanking.Core.Application.Dtos.Transaction.Teller;
using ArtemisBanking.Core.Application.Dtos.Transfer;

namespace ArtemisBanking.Core.Application.Interfaces;

public interface ITransactionService : IGenericService<int, TransactionDto>
{
    /*
     * Estos serian los metodo que implementaria el Client por lo cual no los implemente.
     * Ya que no se como lo irias a usar Ash o Gaviota(Cualquiera que vea el mensaje),
     * el punto es que cuando vayas a utilizarlos descomenta aqui y en la implementacion - Alna

    Task<Result<TransactionDto>> ProcessExpressTransferAsync(ExpressTransferDto dto);
    Task<Result<TransactionDto>> ProcessBeneficiaryTransferAsync(BeneficiaryTransferDto dto);
    Task<Result<TransactionDto>> ProcessCreditCardPaymentAsync(CreditCardPaymentDto dto);
    Task<Result<TransactionDto>> ProcessLoanPaymentAsync(LoanPaymentDto dto);
    Task<Result<TransactionDto>> ProcessAccountTransferAsync(AccountTransferDto dto);
     */

    Task<Result<List<TransactionDto>>> GetByAccountNumberAsync(string accountNumber);
    Task<Result<TransactionDto>> ProcessLoanDisbursementTransfer(LoanDisbursementTransactionDto dto);
    Task<Result<TransactionDto>> ProcessDepositAsync(DepositDto dto);
    Task<Result<TransactionDto>> ProcessWithdrawalAsync(WithdrawalDto dto);
    Task<Result> ProcessTellerTransactionAsync(TellerTransactionDto dto);
    Task<Result<TransactionDto>> ProcessTellerCreditCardPaymentAsync(TellerCreditCardPaymentDto dto);
    Task<Result<TransactionDto>> ProcessTellerLoanPaymentAsync(TellerLoanPaymentDto dto);
    Task<Result<TransactionSummaryDto>> GetTellerTransactionSummaryAsync(string tellerId);
    Task<Result<PaginatedData<TransactionDto>>> GetTransactionsByAccountAsync(string accountNumber, int page, int pageSize);
    Task<Result> TransferAsync(string userId, TransferDto dto);

    /*
     * Un Dto por tipo de trasaccion. Una transaccion por tipo de Dto
     * Task<Result<TransactionDto> ProcessExpressTransferAsync(ExpressTransferDto dto)
     *   internamente validan los datos del DTO
     *   Construyen el Transaction Dto adecuado
     *	 luego llaman al metodo add del servicio para registrar la transaccion
     *   Acreditan y debitan el dinero en las cuentas, tarjetas y pretamos adecuadas
     *   Envian un email si es necesario.
     *
     *  para los metodos de un cajero creo que solamente tienen dos parametros, adecuado al tipo de transaccion:
     * {beneficiario, monto}
     * {Origen, monto}
     * Task<Result<TransactionDto> ProcessExpressTransferAsync(string origin, decimal amount)
     *
     *	Solamente se registra UNA SOLA TRANSACCION EN LA DB, y su tipo {Credito, Debito} se determina desde la perspectiva del ORIGEN
     *
     *  Los metodos de las transacciones deberan de llamar a los metodos de otros servicios para hacer su labor. Transacciones
     * es el servicio central de los cajeros y clientes.
     */

}