namespace ArtemisBanking.Core.Application.Dtos.Login;

public class ConfirmDto
{
    public required string UserId {get;set;}
    public required string Token { get; set; }
}