namespace ArtemisBanking.Core.Application.Interfaces;

public interface IDebtMonitorService
{
     Task ChekcForDueInstallments();
}