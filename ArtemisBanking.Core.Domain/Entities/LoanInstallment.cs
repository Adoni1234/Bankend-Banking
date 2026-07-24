namespace ArtemisBanking.Core.Domain.Entities;

public class LoanInstallment
{
    public required int Id { get; set; }
    public required DateTime PaymentDay { get; set; }
    public required string LoanId { get; set; }
    public required decimal Amount { get; set; }
    public decimal CapitalAmount { get; set; }
    public decimal InterestAmount { get; set; }
    
    public decimal PaidAmount { get; set; }
    public required bool IsPaid { get; set; }
    public required bool IsDue { get; set; } // si esta atrasada
    public Loan? Loan { get; set; }
}