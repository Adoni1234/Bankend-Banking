using ArtemisBanking.Core.Application.Dtos.Transaction;

namespace ArtemisBanking.Core.Application.ViewModels.Teller;

public class TellerDashboardViewModel
{
    public TransactionSummaryDto? Summary { get; set; }
    public string TellerName { get; set; } = string.Empty;
}

