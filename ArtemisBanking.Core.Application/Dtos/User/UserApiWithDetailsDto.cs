using ArtemisBanking.Core.Application.Dtos.SavingAccount;

namespace ArtemisBanking.Core.Application.Dtos.User;

public class UserApiWithDetailsDto
{
    public required string Id { get; set; }
    public required string IdentityCardNumber { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string UserName { get; set; }
    public required string Role { get; set; }
    
    public required SimpleSavingAccountApiDto MainAccount { get; set; }
}