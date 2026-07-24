using ArtemisBanking.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBanking.Infrastructure.Persistence.EntityConfigurations;

public class CreditCardEntityConfiguration : IEntityTypeConfiguration<CreditCard>
{

    public void Configure(EntityTypeBuilder<CreditCard> builder)
    {
        builder.HasKey(x => x.CardNumber);
        builder.Property(x => x.ClientId).IsRequired().HasMaxLength(450);
        builder.Property(x => x.ApprovedByUserId).IsRequired();
        builder.Property(x => x.Balance).IsRequired().HasColumnType("decimal(18,4)");
        builder.Property(x => x.CvcHashed).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.CreditLimit).IsRequired().HasColumnType("decimal(18,4)");
        builder.Property(x => x.ExpirationMonth).IsRequired();
        builder.Property(x => x.ExpirationYear).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        
        builder.HasMany(x => x.CardTransactions)
            .WithOne(x => x.CreditCard)
            .HasForeignKey(x => x.CreditCardNumber)
            .OnDelete(DeleteBehavior.Restrict);
    }
}