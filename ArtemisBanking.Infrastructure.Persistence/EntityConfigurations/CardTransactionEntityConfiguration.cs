using ArtemisBanking.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBanking.Infrastructure.Persistence.EntityConfigurations;

public class CardTransactionEntityConfiguration : IEntityTypeConfiguration<CardTransaction>
{

    public void Configure(EntityTypeBuilder<CardTransaction> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd(); // El ID se genera automáticamente por la base de datos
        
        builder.Property(x => x.Amount).IsRequired().HasColumnType("decimal(18,4)");
        builder.Property(x => x.CommerceId).IsRequired(false); // nullable
        builder.Property(x => x.Date).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.IsCashAdvance).IsRequired();
        builder.Property(x => x.CreditCardNumber).IsRequired();

        builder.HasOne(x => x.Commerce)
            .WithMany(c => c.CardTransactions)
            .HasForeignKey(x => x.CommerceId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(x => x.CreditCard)
            .WithMany(c => c.CardTransactions)
            .HasForeignKey(x => x.CreditCardNumber)
            .OnDelete(DeleteBehavior.Restrict);
    }
}