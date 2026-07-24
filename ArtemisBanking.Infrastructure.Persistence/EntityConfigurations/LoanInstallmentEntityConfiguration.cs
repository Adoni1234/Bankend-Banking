using ArtemisBanking.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBanking.Infrastructure.Persistence.EntityConfigurations;

public class LoanInstallmentEntityConfiguration : IEntityTypeConfiguration<LoanInstallment>
{

    public void Configure(EntityTypeBuilder<LoanInstallment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LoanId);
        builder.Property(x => x.Amount).IsRequired().HasColumnType("decimal(18,4)");
        builder.Property(x => x.PaymentDay).IsRequired();
        builder.Property(x => x.CapitalAmount).IsRequired().HasColumnType("decimal(18,4)");
        builder.Property(x => x.InterestAmount).IsRequired().HasColumnType("decimal(18,4)");
        builder.Property(x => x.PaidAmount).IsRequired().HasColumnType("decimal(18,4)");
        builder.Property(x => x.IsDue).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.IsPaid).IsRequired().HasDefaultValue(false);
        
    }
}