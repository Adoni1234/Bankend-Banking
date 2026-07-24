using ArtemisBanking.Core.Domain.Common.Enums;

namespace ArtemisBanking.Core.Application.ViewModels.User;

public class UserViewModel
{
    public required string Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string IdentityCardNumber { get; set; }
    public required string UserName { get; set; }
    public bool IsVerified { get; set; }
    public required string Role { get; set; }
    public required DateTime RegisteredAt { get; set; }
    public string RoleDisplayName => Role switch
    {
        nameof(Roles.Admin) => "Administrador",
        nameof(Roles.Atm) => "Cajero",
        nameof(Roles.Client) => "Cliente",
        nameof(Roles.Commerce) => "Comercio",
        _ => "Desconocido"
    };
}