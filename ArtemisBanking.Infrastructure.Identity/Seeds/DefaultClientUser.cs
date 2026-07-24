using ArtemisBanking.Core.Domain.Common.Enums;
using ArtemisBanking.Core.Domain.Entities;
using ArtemisBanking.Core.Domain.Interfaces;
using ArtemisBanking.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Identity.Seeds
{
    public static class DefaultClientUser
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager, ISavingAccountRepository savingAccountRepository)
        {
            AppUser user = new()
            {
                IdentityCardNumber = "00000000004",
                FirstName = "Juan",
                LastName = "Client",
                Email = "Client@email.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                UserName = "client",
                RegisteredAt = DateTime.Now 
            };

            string userId = "";
            if (await userManager.Users.AllAsync(u => u.Id != user.Id))
            {
                var entityUser = await userManager.FindByEmailAsync(user.Email);
                if(entityUser == null)
                {
                    await userManager.CreateAsync(user, "123Pa$$word!");
                    await userManager.AddToRoleAsync(user, nameof(Roles.Client));
                   
                    var userJustCreated = await userManager.FindByEmailAsync(user.Email);
                    userId = userJustCreated.Id;
                }
                else
                {
                    userId = entityUser.Id;
                }
            }
            
            var existsMainAccount = await savingAccountRepository.GetAllQueryable()
                .AnyAsync(s => s.ClientId == userId && s.IsPrincipalAccount);

            if (!existsMainAccount)
            {
                var newAccount = new SavingAccount
                {
                    Id = "",
                    ClientId = userId,
                    Balance = 0,
                    CreatedAt = DateTime.Now,
                    AssignedByUserId = userId,
                    IsPrincipalAccount = true,
                    IsActive = true
                };
                await savingAccountRepository.AddAsync(newAccount);
            }
       
        }
    }
}
