using System.Reflection;
using ArtemisBanking.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Persistence.Context;

public class ArtemisContext : DbContext
{
    
    public DbSet<Beneficiary>  Beneficiaries { get; set; } // Beneficiarios
    public DbSet<CardTransaction> CardTransactions { get; set; } // Transacciones de tarjeta
    public DbSet<Commerce> Commerces { get; set; } // comercios
    public DbSet<CreditCard> CreditCards { get; set; } // Tarjetas de credito
    public DbSet<Loan> Loans { get; set; } // Prestamos
    public DbSet<LoanInstallment> LoanInstallments { get; set; } // Cuottas de los prestamos
    public DbSet<SavingAccount> SavingAccounts { get; set; } // Cuentas de ahorro
    public DbSet<Transaction> Transactions { get; set; } // transacciones
    
    public ArtemisContext(DbContextOptions<ArtemisContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasSequence<long>("SeqLoanAndAccountNumbersID")
            .StartsAt(1)
            .IncrementsBy(1)
            .HasMax(999999999)
            .IsCyclic(false);
        
        modelBuilder.HasSequence<long>("SeqCreditCardsID")
            .StartsAt(1)
            .IncrementsBy(1)
            .HasMax(999999999)
            .IsCyclic(false);
        
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
    
}