namespace ArtemisBanking.Core.Application.Dtos.LoanInstallment;

public class SimpleLoanInstallmentApiDto
{
    public required DateTime PaymentDay { get; set; }
    public required decimal Amount { get; set; }
    public decimal PaidAmount { get; set; } // cantidad pagada hasta ahora
    public required bool IsPaid { get; set; }
    public required bool IsDue { get; set; } // si esta atrasada
}