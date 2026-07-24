using ArtemisBanking.Core.Application.Interfaces;
using ArtemisBanking.Core.Domain.Interfaces;
using ArtemisBanking.Core.Domain.Settings;
using ArtemisBanking.Infrastructure.Shared.Services;
using Hangfire;
using Hangfire.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArtemisBanking.Infrastructure.Shared;

public static class ServiceRegistration
{
    public static void AddSharedLayerIoc(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<MailSettings>(config.GetSection("MailSettings"));
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IDebtMonitorService, DebtMonitorService>();

   }

    public static void AddHangfireConfiguration(this IServiceCollection services, IConfiguration config)
    {
        services.AddHangfire(c => { c.UseSqlServerStorage(config.GetConnectionString("DefaultConnection")); });
        services.AddHangfireServer();
    }

    
    public static void UseHangFireJobs(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

        
        recurringJobManager.AddOrUpdate(
            "monitor-deudas",
            Job.FromExpression<IDebtMonitorService>(x => x.ChekcForDueInstallments()), Cron.Daily());
    }

}