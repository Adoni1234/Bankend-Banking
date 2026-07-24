namespace ArtemisBanking.Core.Application.Dtos.Email;

public class PaymentEmailDto
{
    public string To { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public decimal Amount { get; set; } 
    public string LastFourDigitsAccount { get; set; } = string.Empty;
    public string LastFourDigitsCard { get; set; } = string.Empty;
    public string LoanNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string PaymentType { get; set; } = string.Empty; //Esto es para especificar si es pago de un prestamo o Tarjeta de credito.ATT: Alna😒
}