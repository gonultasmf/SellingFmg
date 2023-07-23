using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain;

namespace OrderService.Infrastructure;

public class CardTypeEntityConfiguration : IEntityTypeConfiguration<CardType>
{
    public void Configure(EntityTypeBuilder<CardType> builder)
    {
        builder.ToTable("CardTypes", OrderDbContext.DEFAULT_SCHEMA);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnType("id").ValueGeneratedOnAdd();

        builder.Property(x => x.Id)
            .HasDefaultValue(1)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();
    }
}
