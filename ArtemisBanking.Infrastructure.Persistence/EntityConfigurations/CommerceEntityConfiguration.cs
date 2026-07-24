using ArtemisBanking.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBanking.Infrastructure.Persistence.EntityConfigurations;

public class CommerceEntityConfiguration : IEntityTypeConfiguration<Commerce>
{

    public void Configure(EntityTypeBuilder<Commerce> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Description).IsRequired();
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.Logo).IsRequired();
        builder.Property(x => x.Name).IsRequired();
    }
}