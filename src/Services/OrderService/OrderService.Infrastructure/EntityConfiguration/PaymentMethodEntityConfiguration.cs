using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain;

namespace OrderService.Infrastructure;

internal class PaymentMethodEntityConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("PaymentMethods", OrderDbContext.DEFAULT_SCHEMA);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("id").ValueGeneratedOnAdd();

        builder.Ignore(x => x.DomainEvents);

        builder.Property<int>("BuyerId").IsRequired();

        builder.Property(x => x.CardHolderName)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("CardHolderName")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Alias)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("Alias")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.CardNumber)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("CardNumber")
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(x => x.Expiration)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("Expiration")
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(x => x.CardTypeId)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("CardTypeId")
            .IsRequired();

        builder.HasOne(x => x.CardType)
            .WithMany()
            .HasForeignKey(x => x.CardTypeId);
    }
}
