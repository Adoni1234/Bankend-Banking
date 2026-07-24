using ArtemisBanking.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBanking.Infrastructure.Persistence.EntityConfigurations;

public class TransactionEntityConfiguration : IEntityTypeConfiguration<Transaction>
{

    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd(); // El ID se genera automáticamente por la base de datos
        
        builder.Property(x => x.Amount).IsRequired().HasColumnType("decimal(18,4)");
        builder.Property(x => x.Beneficiary).IsRequired();
        builder.Property(x => x.Origin).IsRequired();
        builder.Property(x => x.Date).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.CreatedById).IsRequired();
        
        builder.HasOne(t => t.SavingAccount)
            .WithMany(sc => sc.Transactions)
            .HasForeignKey(t => t.AccountNumber);
    }
}