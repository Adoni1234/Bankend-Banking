using ArtemisBanking.Core.Domain.Common.Enums;
using ArtemisBanking.Core.Domain.Entities;
using ArtemisBanking.Core.Domain.Interfaces;
using ArtemisBanking.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Identity.Seeds
{
    public static class DefaultCommerceUser
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager, ICommerceRepository commerceRepository, ISavingAccountRepository savingAccountRepository)
        {
            
            Commerce commerce = new  Commerce
            {
                Id = 0,
                Name = "Supermercados Bravos",
                Description = "Por alguna razon, tenemos un reguero de productos.",
                Logo = "https://i0.wp.com/directoriodominicano.net/wp-content/uploads/2024/08/supermercado-dominicano-bravo.png?fit=375%2C245&ssl=1",
                IsActive = true 
            };

            int commerceId = 0;
            var existsCommerce = await commerceRepository.GetAllQueryable().FirstOrDefaultAsync(c => c.Name == commerce.Name);
            if (existsCommerce == null)
            {
                var commerceDb = await commerceRepository.AddAsync(commerce);
                commerceId = commerceDb.Id;
            }
            else
            {
                commerceId = existsCommerce.Id;
            }
            
            AppUser user = new()
            {
                IdentityCardNumber = "00000000002",
                FirstName = "John",
                LastName = "Commerce",
                Email = "commerce@email.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                UserName = "commerce",
                RegisteredAt = DateTime.Now ,
                CommerceId = commerceId
            };

            string userId = "";
            if (await userManager.Users.AllAsync(u => u.Id != user.Id))
            {
                var entityUser = await userManager.FindByEmailAsync(user.Email);
                if(entityUser == null)
                {
                   await userManager.CreateAsync(user, "123Pa$$word!");
                   await userManager.AddToRoleAsync(user, nameof(Roles.Commerce));
                   
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
