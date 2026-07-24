using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Core.Application.ViewModels.Login
{
    public class ResetPasswordRequestViewModel
    {
        [Required]
        [DataType(DataType.Text)]
        public required string Id { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public required string Token { get; set; }

        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Password must match")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }
    }
}
