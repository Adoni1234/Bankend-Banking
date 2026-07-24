using Microsoft.AspNetCore.Identity;

namespace ArtemisBanking.Infrastructure.Identity.Entities;

public class AppUser : IdentityUser
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string IdentityCardNumber { get; set; }
    
    public int? CommerceId { get; set; }
    public required DateTime RegisteredAt { get; set; } 
}