using System.Reflection;
using ArtemisBanking.Core.Domain.Interfaces;
using ArtemisBanking.Infrastructure.Persistence.Context;
using ArtemisBanking.Infrastructure.Persistence.Repositories;
using ArtemisBanking.Infrastructure.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArtemisBanking.Infrastructure.Persistence;

public static class ServiceRegistration
{
    public static void AddPersistenceLayerIoc(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ArtemisContext>(
            opt =>
            {
                opt.EnableSensitiveDataLogging();
                opt.UseSqlServer(
                    config.GetConnectionString("DefaultConnection"),
                    m => m.MigrationsAssembly(typeof(ArtemisContext).Assembly.FullName)
                );
            } 
        );
        
        services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
        services.AddScoped<IBeneficiaryRepository, BeneficiaryRepository>();
        services.AddScoped<ICardTransactionRepository, CardTransactionRepository>();
        services.AddScoped<ICommerceRepository, CommerceRepository>();
        services.AddScoped<ICreditCardRepository, CreditCardRepository>();
        services.AddScoped<ILoanInstallmentRepository, LoanInstallmentRepository>();
        services.AddScoped<ILoanRepository, LoanRepository>();
        services.AddScoped<ISavingAccountRepository, SavingAccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IIdentifierService, IdentifierService>();
    }
}