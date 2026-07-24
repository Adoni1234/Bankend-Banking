using System.Reflection;
using ArtemisBanking.Core.Application.Interfaces;
using ArtemisBanking.Core.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArtemisBanking.Core.Application
{
    public static class ServiceRegistration
    {
        //Extension method - Decorator pattern
        public static void AddApplicationLayerIoc(this IServiceCollection services, IConfiguration config)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddScoped<IBeneficiaryService, BeneficiaryService>();
            services.AddScoped<ICreditCardService, CreditCardService>();
            services.AddScoped<ICardTransactionService, CardTransactionService>();
            services.AddScoped<ICommerceService, CommerceService>();
            services.AddScoped<ICreditCardService, CreditCardService>();
            services.AddScoped<ILoanService, LoanService >();
            services.AddScoped<ILoanInstallmentService, LoanInstallmentService>();
            services.AddScoped<ISavingAccountService, SavingAccountService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IRiskService, RiskService>();
            services.AddScoped<IDashboardService, DashboardService>();
        }
    }
}
