using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain;

namespace OrderService.Infrastructure;

public class OrderEntityConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders", OrderDbContext.DEFAULT_SCHEMA);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Ignore(x => x.DomainEvents);

        builder.OwnsOne(x => x.Address, a =>
        {
            a.WithOwner();
        });

        builder.Property<int>("orderStatusId")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("OrderStatusId")
            .IsRequired();

        var navigation = builder.Metadata.FindNavigation(nameof(Order.OrderItems));
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne(x => x.Buyer)
            .WithMany()
            .HasForeignKey(x => x.BuyerId);

        builder.HasOne(x => x.OrderStatus)
            .WithMany()
            .HasForeignKey("orderStatusId");
    }
}
