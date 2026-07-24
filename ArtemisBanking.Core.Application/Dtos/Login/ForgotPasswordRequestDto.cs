namespace ArtemisBanking.Core.Application.Dtos.Login
{
    public class ForgotPasswordRequestDto
    {
        public required string UserName { get; set; }
        public required string Origin { get; set; }
    }
}
