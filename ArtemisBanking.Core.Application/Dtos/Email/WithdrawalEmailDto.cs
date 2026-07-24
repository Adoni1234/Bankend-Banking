namespace ArtemisBanking.Core.Application.Dtos.Email;

public class WithdrawalEmailDto
{
    //Este dto es para los retiros
    public string To { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string LastFourDigits { get; set; } = string.Empty;
    public DateTime WithdrawalDate { get; set; }
}