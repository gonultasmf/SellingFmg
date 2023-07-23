using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain;

namespace OrderService.Infrastructure;

public class BuyerEntityConfiguration : IEntityTypeConfiguration<Buyer>
{
    public void Configure(EntityTypeBuilder<Buyer> builder)
    {
        builder.ToTable("Buyers", OrderDbContext.DEFAULT_SCHEMA);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Ignore(x => x.DomainEvents);

        builder.Property(x => x.Name).HasColumnType("name").HasColumnType("varchar").HasMaxLength(100);

        builder.HasMany(x => x.PaymentMethods)
            .WithOne()
            .HasForeignKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        var navigation = builder.Metadata.FindNavigation(nameof(Buyer.PaymentMethods));
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
