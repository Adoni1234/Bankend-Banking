namespace ArtemisBanking.Core.Application.ViewModels.AdminHome;

public class DashboardAdminViewModel
{
    public required int TotalTransactions  {get; set;}
    public required int TodayTransactions { get; set; }
    public required int TotalPayments { get; set; }
    public required int TodayPayments { get; set; }
    public required int ActiveClients { get; set; }
    public required int InactiveClients { get; set; }
    public required int TotalProducts { get; set; }
    public required int ActiveLoans { get; set; }
    public required int ActiveCreditCards { get; set; }
    public required int ActiveSavingsAccounts { get; set; }
    public required decimal AverageClientDebt { get; set; }
}
