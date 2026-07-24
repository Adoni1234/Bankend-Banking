using ArtemisBanking.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Identity;

namespace ArtemisBanking.Infrastructure.Identity.Seeds
{
    public static class DefaultRoles
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            await roleManager.CreateAsync(new IdentityRole(nameof(Roles.Admin)));
            await roleManager.CreateAsync(new IdentityRole(nameof(Roles.Client)));
            await roleManager.CreateAsync(new IdentityRole(nameof(Roles.Atm)));
            await roleManager.CreateAsync(new IdentityRole(nameof(Roles.Commerce)));
        }
    }
}
