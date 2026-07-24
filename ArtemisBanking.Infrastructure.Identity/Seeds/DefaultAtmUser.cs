using ArtemisBanking.Core.Domain.Common.Enums;
using ArtemisBanking.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Identity.Seeds
{
    public static class DefaultAtmUser
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager)
        {
            AppUser user = new()
            {
                IdentityCardNumber = "00000000003",
                FirstName = "Marcos",
                LastName = "Atm",
                Email = "Atm@email.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                UserName = "atm",
                RegisteredAt = DateTime.Now 
            };

            if (await userManager.Users.AllAsync(u => u.Id != user.Id))
            {
                var entityUser = await userManager.FindByEmailAsync(user.Email);
                if(entityUser == null)
                {
                    await userManager.CreateAsync(user, "123Pa$$word!");
                    await userManager.AddToRoleAsync(user, nameof(Roles.Atm));
                }
            }
       
        }
    }
}
