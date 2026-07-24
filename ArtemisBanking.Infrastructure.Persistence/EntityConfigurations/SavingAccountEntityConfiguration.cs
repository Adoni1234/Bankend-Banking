using ArtemisBanking.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBanking.Infrastructure.Persistence.EntityConfigurations;

public class SavingAccountEntityConfiguration : IEntityTypeConfiguration<SavingAccount>
{

    public void Configure(EntityTypeBuilder<SavingAccount> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Balance).IsRequired().HasColumnType("decimal(18,4)");
        builder.Property(x => x.ClientId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.AssignedByUserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsPrincipalAccount).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
    }
}