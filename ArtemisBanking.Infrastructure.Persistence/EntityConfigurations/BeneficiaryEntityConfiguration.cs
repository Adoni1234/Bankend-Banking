using ArtemisBanking.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBanking.Infrastructure.Persistence.EntityConfigurations;

public class BeneficiaryEntityConfiguration : IEntityTypeConfiguration<Beneficiary>
{
    public void Configure(EntityTypeBuilder<Beneficiary> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(450);
        builder.Property(x => x.SavingAccountId).IsRequired();

        builder.HasOne(b => b.SavingAccount).WithOne().OnDelete(DeleteBehavior.Restrict);
    }
}