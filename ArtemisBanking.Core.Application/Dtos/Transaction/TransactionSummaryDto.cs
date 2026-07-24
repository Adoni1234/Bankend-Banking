namespace ArtemisBanking.Core.Application.Dtos.Transaction;

public class TransactionSummaryDto
{
    public int TotalTransactions { get; set; }
    public int TodayTransactions { get; set; }
    public int TotalPayments { get; set; }
    public int TodayPayments { get; set; }
    public int TotalDeposits { get; set; }
    public int TodayDeposits { get; set; }
    public int TotalWithdrawals { get; set; }
    public int TodayWithdrawals { get; set; }
}