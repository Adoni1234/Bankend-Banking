namespace ArtemisBanking.Core.Application.Dtos.User;

public class UserDto
{
    public required string Id { get; set; }
    public required string IdentityCardNumber { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string UserName { get; set; }
    
    public int? CommerceId { get; set; }
    public required DateTime RegisteredAt { get; set; }
    public bool? IsVerified { get; set; }
    public required string Role { get; set; }
}