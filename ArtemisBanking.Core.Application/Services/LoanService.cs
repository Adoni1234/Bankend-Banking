using ArtemisBanking.Core.Application.Dtos.Email;
using ArtemisBanking.Core.Application.Dtos.Loan;
using ArtemisBanking.Core.Application.Dtos.LoanInstallment;
using ArtemisBanking.Core.Application.Dtos.Transaction;
using ArtemisBanking.Core.Application.Dtos.User;
using ArtemisBanking.Core.Application.Enums;
using ArtemisBanking.Core.Application.Helpers;
using ArtemisBanking.Core.Application.Interfaces;
using ArtemisBanking.Core.Domain.Common.Enums;
using ArtemisBanking.Core.Domain.Entities;
using ArtemisBanking.Core.Domain.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Core.Application.Services;

public class LoanService : GenericServices<string, Loan, LoanDto>, ILoanService
{
    private readonly ILoanRepository _loanRepository;
    private readonly ILoanInstallmentRepository _loanInstallmentRepository;
    private readonly ITransactionRepository _transactionRepository;

    private readonly IEmailService _emailService;
    private readonly ISavingAccountService _savingAccountService;
    private readonly ILoanInstallmentService _loanInstallmentService;
    private readonly IBaseAccountService _accountServiceForWebApp;
    private readonly IRiskService _riskService;
    private readonly IMapper _mapper;

    public LoanService(ILoanInstallmentRepository loanInstallmentRepository,
        ITransactionRepository transactionRepository, IEmailService emailService,
        ISavingAccountService savingAccountService, ILoanInstallmentService loanInstallmentService,
        IBaseAccountService accountServiceForWebApp, IRiskService riskService, IMapper mapper,
        ILoanRepository loanRepository) : base(loanRepository, mapper)
    {
        _loanInstallmentRepository = loanInstallmentRepository;
        _transactionRepository = transactionRepository;
        _emailService = emailService;
        _savingAccountService = savingAccountService;
        _loanInstallmentService = loanInstallmentService;
        _accountServiceForWebApp = accountServiceForWebApp;
        _riskService = riskService;
        _mapper = mapper;
        _loanRepository = loanRepository;
    }


    public override async Task<Result<LoanDto>> AddAsync(LoanDto dtoModel)
    {
        if (await UserHasActiveLoanAsync(dtoModel.ClientId))
        {
            return Result<LoanDto>.Fail("El cliente ya tiene un préstamo activo");
        }

        var userWhoApproved = await _accountServiceForWebApp.GetUserById(dtoModel.ApprovedByUserId);
        if (userWhoApproved is null)
        {
            return Result<LoanDto>.Fail("No se encontro el usuario");
        }

        if (userWhoApproved.Role != nameof(Roles.Admin))
        {
            return Result<LoanDto>.Fail("Usted no esta autorizado para asignar prestamos");
        }

        var validTermMonths = new[] { 6, 12, 18, 24, 30, 36, 42, 48, 54, 60 };
        if (!validTermMonths.Contains(dtoModel.TermMonths))
        {
            return Result<LoanDto>.Fail("Los plazos solamente pueden ser de: 6, 12, 18, 24, 30, 36, 42, 48, 54, 60");
        }

        // creamos el prestamo y sus cuotas
        var createLoanResult = await base.AddAsync(dtoModel);
        if (createLoanResult.IsFailure)
        {
            return createLoanResult;
        }

        var installments = GenerateLoansInstallments(createLoanResult.Value!);
        var createLoanInstallmentResult = await _loanInstallmentService.AddRangeAsync(installments);
        if (createLoanInstallmentResult.IsFailure)
        {
            return Result<LoanDto>.Fail(createLoanInstallmentResult.GeneralError!);
        }

        var mainAccountResult = await _savingAccountService.GetMainAccountByUserIdAsync(dtoModel.ClientId);
        if (mainAccountResult.IsFailure)
        {
            return Result<LoanDto>.Fail(mainAccountResult.GeneralError!);
        }

        // registramos la transaccion
        var transaction = new Transaction
        {
            Id = 0,
            Amount = dtoModel.Amount,
            CreatedById = userWhoApproved.Id,
            AccountNumber = mainAccountResult.Value!.Id,
            Type = TransactionType.Credit,
            Beneficiary = mainAccountResult.Value!.Id,
            Origin = createLoanResult.Value!.Id,
            Date = DateTime.Now,
            SubType = TransactionSubType.Deposit,
            Status = TransactionStatus.Approved,
        };

        var transactionResult = await _transactionRepository.AddAsync(transaction);
        var depositResult =
            await _savingAccountService.DepositToAccountAsync(mainAccountResult.Value!.Id, dtoModel.Amount);

        if (depositResult.IsFailure)
            return Result<LoanDto>.Fail(depositResult.GeneralError!);

        // para el email
        var client = await _accountServiceForWebApp.GetUserById(dtoModel.ClientId);
        if (client is null)
        {
            return Result<LoanDto>.Fail("no se encontro el cliente");
        }

        var newMontly = MontlyPayment.Calculate(
            createLoanResult.Value!.Amount,
            createLoanResult.Value!.AnualRate,
            createLoanResult.Value.TermMonths
        );

        await _emailService.SendTemplateEmailAsync(new EmailTemplateDataDto()
        {
            Type = EmailType.LoanApproved,
            To = client.Email,
            Variables =
            {
                ["LoanNumber"] = createLoanResult.Value.Id,
                ["Amount"] = createLoanResult.Value.Amount.ToString("N2"),
                ["Term"] = createLoanResult.Value.TermMonths.ToString(),
                ["InterestRate"] = createLoanResult.Value!.AnualRate.ToString("N2"),
                ["MonthlyPayment"] = newMontly.ToString("N2"),
            }
        });

        return createLoanResult;
    }

    public Task<bool> UserHasActiveLoanAsync(string userId)
    {
        // Los prestamos NO COMPLETADOS, estan activos
        return _loanRepository.GetAllQueryable().AsNoTracking()
            .AnyAsync(l => l.ClientId == userId && l.Completed == false);
    }

    public async Task<Result<List<ClientsWithDebtDto>>> GetClientsWithoutActiveLoan(string? identityCardNumber = null)
    {
        var allClientsResult = await _accountServiceForWebApp.GetAllUserIdsOfRole(Roles.Client);
        var allClientIds = allClientsResult.Value!.ToList();

        var clientsWithActiveLoans = await _loanRepository.GetAllQueryable()
            .AsNoTracking()
            .Where(l => !l.Completed)
            .Select(l => l.ClientId)
            .Distinct()
            .ToListAsync();

        var clientsWithoutActiveLoansIds = allClientIds.Except(clientsWithActiveLoans).ToList();


        if (!clientsWithoutActiveLoansIds.Any())
        {
            return Result<List<ClientsWithDebtDto>>.Ok(new List<ClientsWithDebtDto>());
        }

        return await _riskService.GetDebtOfTheseUsers(clientsWithoutActiveLoansIds, identityCardNumber);
    }


    public async Task<PaginatedData<LoanSummaryDto>> GetLoansPagedAsync(
        int page,
        int pageSize,
        string? identityCardNumber = null,
        bool? isCompleted = null)
    {
        UserDto? client = null;
        if (!string.IsNullOrEmpty(identityCardNumber))
        {
            var clientResult = await _accountServiceForWebApp.GetByIdentityCardNumber(identityCardNumber);

            if (clientResult.IsFailure || clientResult.Value is null)
                return new PaginatedData<LoanSummaryDto>([], 0, page, pageSize);

            client = clientResult.Value;
        }

        var query = _loanRepository.GetAllQueryable().AsNoTracking();

        if (client is not null)
            query = query.Where(l => l.ClientId == client.Id);

        if (isCompleted.HasValue)
            query = query.Where(l => l.Completed == isCompleted.Value);

        var usersIds = await query.Select(l => l.ClientId).Distinct().ToListAsync();
        var usersResult = await _accountServiceForWebApp.GetUsersByIds(usersIds);
        var usersDict = usersResult.Value!.ToDictionary(user => user.Id, user => user);

        var items = query
            .OrderByDescending(l => l.Completed)
            .ThenByDescending(l => l.CreatedAt)
            .Select(l => new LoanSummaryDto
            {
                Id = l.Id,
                Amount = l.Amount,
                CreatedAt = l.CreatedAt,
                Completed = l.Completed,
                ClientId = l.ClientId,
                ApprovedByUserId = l.ApprovedByUserId,
                TermMonths = l.TermMonths,
                InstallmentsCount = l.LoanInstallments.Count(),
                InstallmentsPaidCount = l.LoanInstallments.Count(installment => installment.IsPaid),
                RemainingBalanceToPay = Math.Max( // Esto se quedaba en -1, tuve que poner esto para evitarlo
                    0,
                    Math.Round(
                        l.Amount - l.LoanInstallments
                            .Where(inst => inst.IsPaid)
                            .Sum(inst => inst.CapitalAmount),
                        2
                    )
                ),
                AnualRate = l.AnualRate,
                IsDue = l.IsDue,
                Client = usersDict[l.ClientId],
            });


        var data = PaginatedData<LoanSummaryDto>.Create(items, page, pageSize);
        return data;
    }

    public async Task<Result<LoanDto>> GetAmortizationTableAsync(string loanId)
    {
        var loan = await _loanRepository.GetAllQueryable().AsNoTracking()
            .Include(l => l.LoanInstallments)
            .FirstOrDefaultAsync(l => l.Id == loanId);

        if (loan is null)
        {
            return Result<LoanDto>.Fail("Loan not found");
        }

        return Result<LoanDto>.Ok(_mapper.Map<LoanDto>(loan));
    }

    public async Task<Result<LoanDto>> UpdateInterestRateAsync(string loanId, decimal newRate)
    {
        var loanResult = await GetByIdAsync(loanId);
        if (loanResult.IsFailure)
        {
            return loanResult;
        }

        var loan = loanResult.Value!;
        loan.AnualRate = newRate;
        var updateResult = await UpdateAsync(loan.Id, loan);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await RecalculateInstallmentsAndUpdate(loan.Id, newRate);


        // Email
        var client = await _accountServiceForWebApp.GetUserById(loan.ClientId);
        if (client is null)
        {
            return Result<LoanDto>.Fail("no se encontro al usuario");
        }

        var newMontly = MontlyPayment.Calculate(loan.Amount, newRate, loan.TermMonths);
        await _emailService.SendTemplateEmailAsync(new EmailTemplateDataDto
        {
            Type = EmailType.LoanRateUpdated,
            To = client.Email,
            Variables =
            {
                ["LoanId"] = loan.Id,
                ["LoanAmount"] = loan.Amount.ToString("N2"),
                ["NewRate"] = newRate.ToString("N2"),
                ["NewMonthlyPayment"] = newMontly.ToString("n2"),
                ["Date"] = DateTime.Now.ToShortDateString(),
            }
        });

        return Result<LoanDto>.Ok(updateResult.Value!);
    }

    // Paga la cantidad maxima de cuotas posibles con el monto dado y devuelve la cantidad REAL de dinero usado del monto
    public async Task<Result<decimal>> PayAsync(string loanId, decimal amount)
    {
        var loan = await _loanRepository.GetByIdAsync(loanId);
        if (loan is null)
            return Result<decimal>.Fail("Ese préstamo no existe");

        if (loan.Completed)
            return Result<decimal>.Fail("El préstamo ya está pagado");

        var installments = await _loanInstallmentRepository.GetAllQueryable()
            .Where(inst => inst.LoanId == loanId && !inst.IsPaid)
            .OrderBy(inst => inst.PaymentDay)
            .ToListAsync();

        var pendingTotal = installments.Sum(i => i.Amount);

        var amountToUse = Math.Min(amount, pendingTotal);

        var remaining = amountToUse;
        decimal moneyUsed = 0;

        for (int i = 0; i < installments.Count; i++)
        {
            if (remaining <= 0)
                break;

            var installment = installments[i];

            if (remaining >= installment.Amount)
            {
                // pagar cuota completa
                installment.PaidAmount += installment.Amount;
                remaining -= installment.Amount;
                moneyUsed += installment.Amount;

                installment.IsPaid = true;
                installment.IsDue = false;
            }
            else
            {
                // pago parcial
                installment.PaidAmount += remaining;
                moneyUsed += remaining;
                remaining = 0;
            }
        }

        await _loanInstallmentRepository.SaveChangesAsync();

        // comprobar si todas están pagadas
        var allPaid = await _loanInstallmentRepository
            .GetAllQueryable()
            .Where(inst => inst.LoanId == loanId)
            .AllAsync(inst => inst.IsPaid);

        if (allPaid)
        {
            loan.Completed = true;
            loan.IsDue = false;
            await _loanRepository.UpdateAsync(loan.Id, loan);
        }

        return Result<decimal>.Ok(moneyUsed);
    }


    public async Task RecalculateInstallmentsAndUpdate(string loanId, decimal newAnnualRate)
    {
        var loan = await _loanRepository.GetAllQueryable()
            .Include(l => l.LoanInstallments)
            .FirstOrDefaultAsync(l => l.Id == loanId);

        // 1. Filtrar y modificar cuotas futuras
        var futureInstallments = loan.LoanInstallments
            .Where(i => i.PaymentDay > DateTime.Now && !i.IsPaid)
            .OrderBy(i => i.PaymentDay)
            .ToList();

        // 2. Recalcular valores
        decimal remainingCapital = futureInstallments.Sum(i => i.CapitalAmount);
        var newValues = RecalculateInstallments(remainingCapital, newAnnualRate, futureInstallments.Count);

        // 3. Aplicar cambios (EF los detecta automáticamente)
        for (int i = 0; i < futureInstallments.Count; i++)
        {
            futureInstallments[i].Amount = newValues[i].Amount;
            futureInstallments[i].CapitalAmount = newValues[i].CapitalAmount;
            futureInstallments[i].InterestAmount = newValues[i].InterestAmount;
        }

        await _loanRepository.SaveChangesAsync();
    }

    // Metodo axiliar para recalcular las cuotas luego de actualizar la tasa 
    private static List<LoanInstallment> RecalculateInstallments(
        decimal remainingCapital, decimal newAnnualRate, int remainingTerm)
    {
        var newInstallments = new List<LoanInstallment>();
        decimal monthlyRate = newAnnualRate / 100 / 12;

        // Recalcular cuota fija con nuevo interés
        decimal newFixedPayment = MontlyPayment.Calculate(remainingCapital, newAnnualRate, remainingTerm);

        decimal remainingBalance = remainingCapital;

        for (int i = 1; i <= remainingTerm; i++)
        {
            decimal interestAmount = Math.Round(remainingBalance * monthlyRate, 2);
            decimal capitalAmount = newFixedPayment - interestAmount;

            // Ajuste última cuota
            if (i == remainingTerm)
            {
                capitalAmount = remainingBalance;
                newFixedPayment = capitalAmount + interestAmount;
            }

            newInstallments.Add(new LoanInstallment
            {
                Id = 0,
                PaymentDay = default,
                LoanId = "",
                IsPaid = false,
                IsDue = false,

                Amount = Math.Round(newFixedPayment, 2),
                CapitalAmount = Math.Round(capitalAmount, 2),
                InterestAmount = Math.Round(interestAmount, 2)
            });

            remainingBalance -= capitalAmount;
        }

        return newInstallments;
    }

    // Usado para crear las cuotas de un prestamo recien creado
    private static List<LoanInstallmentDto> GenerateLoansInstallments(LoanDto loan)
    {
        var installments = new List<LoanInstallmentDto>();

        // 1. Calcular tasa de interés mensual
        decimal monthlyRate = loan.AnualRate / 100 / 12;

        // 2. Calcular cuota fija (fórmula sistema francés)
        var fixedPayment = MontlyPayment.Calculate(loan.Amount, loan.AnualRate, loan.TermMonths);
        int term = loan.TermMonths;

        decimal fixedPaymentDecimal = Math.Round(fixedPayment, 2);

        // 3. Generar cada cuota
        decimal remainingBalance = loan.Amount;
        DateTime dueDate = DateTime.Now.AddMonths(1); // Primera cuota en 1 mes

        for (int i = 1; i <= term; i++)
        {
            // Calcular interés de esta cuota
            decimal interestAmount = Math.Round(remainingBalance * monthlyRate, 2);

            // Calcular capital de esta cuota
            decimal capitalAmount = fixedPaymentDecimal - interestAmount;

            // Ajustar última cuota para evitar desfases por redondeo
            if (i == term)
            {
                capitalAmount = remainingBalance;
                fixedPaymentDecimal = capitalAmount + interestAmount;
            }

            // Crear la cuota
            var installment = new LoanInstallmentDto
            {
                Id = 0,
                Amount = Math.Round(fixedPaymentDecimal, 2),
                CapitalAmount = Math.Round(capitalAmount),
                InterestAmount = Math.Round(interestAmount),
                LoanId = loan.Id,
                PaymentDay = dueDate,
                IsPaid = false,
                IsDue = false,
            };

            installments.Add(installment);

            // Actualizar balance y fecha para siguiente cuota
            remainingBalance -= capitalAmount;
            dueDate = dueDate.AddMonths(1);
        }

        return installments;
    }

    public async Task<List<LoanDto>> GetByUserIdAsync(string userId)
    {
        var list = await _loanRepository.GetAllAsync();

        return list
            .Where(l => l.ClientId == userId)
            .Select(l => _mapper.Map<LoanDto>(l))
            .ToList();
    }

}