using ArtemisBanking.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBanking.Infrastructure.Persistence.EntityConfigurations;

public class LoanEntityConfiguration : IEntityTypeConfiguration<Loan>
{

    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ApprovedByUserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.ClientId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Amount).IsRequired().HasColumnType("decimal(18,4)");
        builder.Property(x => x.AnualRate).IsRequired().HasColumnType("decimal(18,4)");
        builder.Property(x => x.Completed).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.IsDue).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).IsRequired();
        
        builder.HasMany(x => x.LoanInstallments)
            .WithOne(x => x.Loan)
            .HasForeignKey(x => x.LoanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}