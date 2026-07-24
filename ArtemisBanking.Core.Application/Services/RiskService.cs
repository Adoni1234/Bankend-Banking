using ArtemisBanking.Core.Application.Dtos.Loan;
using ArtemisBanking.Core.Application.Interfaces;
using ArtemisBanking.Core.Domain.Common.Enums;
using ArtemisBanking.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Core.Application.Services;

public class RiskService : IRiskService
{
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly ILoanInstallmentRepository _loanInstallmentRepository;
    private readonly ICardTransactionRepository _cardTransactionRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IBaseAccountService _accountServiceForWebApp;

    public RiskService(ITransactionRepository transactionRepository, IBaseAccountService accountServiceForWebApp,
        ICardTransactionRepository cardTransactionRepository, ILoanInstallmentRepository loanInstallmentRepository, ILoanRepository loanRepository, ICreditCardRepository creditCardRepository)
    {
        _transactionRepository = transactionRepository;
        _accountServiceForWebApp = accountServiceForWebApp;
        _cardTransactionRepository = cardTransactionRepository;
        _loanInstallmentRepository = loanInstallmentRepository;
        _loanRepository = loanRepository;
        _creditCardRepository = creditCardRepository;
    }


    public async Task<decimal> GetSystemAverageClientDebtAsync()
    {
        // Total de clientes activos
        var clientsResult = await _accountServiceForWebApp.GetAllUserIdsOfRole(Roles.Client);
        var clients = clientsResult.Value!;


        if (clients.Count == 0) return 0;

        // deuda de prestamos: Sumar capital pendiente de TODAS las cuotas pendientes
        decimal loansDebt = _loanInstallmentRepository.GetAllQueryable().AsNoTracking()
            .Where(inst => !inst.IsPaid)
            .Sum(inst => inst.CapitalAmount);


        // deudas de tarjetas: Sumar consumos aprobados de TODAS las tarjetas activas
        var cardUsages = _cardTransactionRepository.GetAllQueryable().AsNoTracking()
            .Include(ct => ct.CreditCard)
            .Where(ct => ct.CreditCard.IsActive && ct.Status == CreditCardTransactionStatus.Approved)
            .Sum(ct => ct.Amount); // Los intereses ya están aplicados al monto

        // pagos a tarjetas: Sumar TODOS los pagos a tarjetas
        var creditCardPayments = _transactionRepository.GetAllQueryable().AsNoTracking()
            .Where(t => t.Beneficiary.Length == 16 && // 16 dígitos = tarjeta
                        t.SubType == TransactionSubType.CreditCardPayment &&
                        t.Status == TransactionStatus.Approved)
            .Sum(t => t.Amount);

        // 5. Cálculo final
        var totalDebt = loansDebt + (cardUsages - creditCardPayments);

        return totalDebt / clients.Count();
    }

    public async Task<decimal> CalculateClientTotalDebt(string userId)
    {
        var loanDebt = await CalculateClientDebtOfAllLoansAsync(userId);
        var creditDebt = await CalculateClientDebtOfAllCreditCardsAsync(userId);
        return loanDebt + creditDebt;
    }

    // calcula la deuda total de prestamos del cliente. Se incluyen todos los prestamos
    public async Task<decimal> CalculateClientDebtOfAllLoansAsync(string clientId)
    {
        var totalLoanDebt = await _loanRepository.GetAllQueryable().AsNoTracking()
            .Where(l => l.ClientId == clientId  && l.Completed == false)
            .SelectMany(l => l.LoanInstallments)
            .Where(inst => !inst.IsPaid)
            .SumAsync(i => i.CapitalAmount);

        return totalLoanDebt;
    }
    
    // Calcula la deuda total de tarjetas del cliente. Se incluyen todas las tarjetas
    public async Task<decimal> CalculateClientDebtOfAllCreditCardsAsync(string clientId)
    {
        // Obtener tarjetas del cliente
        var creditCardsOfClient = _creditCardRepository.GetAllQueryable().AsNoTracking()
            .Where(c => c.ClientId == clientId && c.IsActive);

        var cardNumbersOfClient = await creditCardsOfClient
            .Select(c => c.CardNumber)
            .ToListAsync();
    
        // Sumar consumos 
        var totalCardUsagesOfClient = await creditCardsOfClient
            .SelectMany(c => c.CardTransactions)
            .Where(ct => ct.Status == CreditCardTransactionStatus.Approved)
            .SumAsync(ct => ct.Amount); // amount incluye los intereses en caso de avance

        // Sumar pagos recibidos
        var totalCreditCardsPaymentsOfUser = await _transactionRepository.GetAllQueryable().AsNoTracking()
            .Where(t => t.Beneficiary.Length == 16 && 
                        cardNumbersOfClient.Contains(t.Beneficiary) &&
                        t.SubType == TransactionSubType.CreditCardPayment &&
                        t.Status == TransactionStatus.Approved)
            .SumAsync(t => t.Amount);

        // Deuda neta = Consumos - Pagos
        return totalCardUsagesOfClient - totalCreditCardsPaymentsOfUser;
    }
    
    // Devuelve el capital mas los intereses del prestamo (CAPITAL + INTERESES)
    public decimal CalculateTotalLoanDebt(decimal capital, decimal anualRate, int termMonths)
    {
        // 1. Convertir a double para cálculos matemáticos
        double capitalD = (double)capital;
        double anualRateLd = (double)anualRate / 12 / 100;
        int termMonthsD = termMonths;

        // 2. Calcular cuota mensual
        double cuote = capitalD * 
                       (anualRateLd * Math.Pow(1 + anualRateLd, termMonthsD)) / 
                       (Math.Pow(1 + anualRateLd, termMonthsD) - 1);

        // 3. Calcular total que pagará el cliente
        double capitalWithInterests = cuote * termMonthsD;

        // 4. Deuda total para riesgo = Capital + Intereses
        decimal deudaTotal = capital + (decimal)(capitalWithInterests - capitalD);
        
        return deudaTotal;
    }
    
    
    public async Task<Result<List<ClientsWithDebtDto>>> GetDebtOfTheseUsers(List<string> usersIds, string? identityCardNumber = null)
    {
        var usersResult = await _accountServiceForWebApp.GetUsersByIds(usersIds);
        var users = usersResult.Value!;
        
        if (!users.Any())
        {
            return Result<List<ClientsWithDebtDto>>.Ok(new List<ClientsWithDebtDto>());
        }

        if (!string.IsNullOrWhiteSpace(identityCardNumber))
        {
            users = users
                .Where(u => u.IdentityCardNumber.Contains(identityCardNumber, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        
        var clientsWithoutLoans = new List<ClientsWithDebtDto>();
        foreach (var u in users)
        {
            var debt = await CalculateClientTotalDebt(u.Id);

            clientsWithoutLoans.Add(new ClientsWithDebtDto
            {
                Client = u,
                Debt = debt
            });
        }

        return Result<List<ClientsWithDebtDto>>.Ok(clientsWithoutLoans);
    }
}