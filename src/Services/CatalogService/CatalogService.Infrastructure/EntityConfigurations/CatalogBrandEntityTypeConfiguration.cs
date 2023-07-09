using CatalogService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Infrastructure;

public class CatalogBrandEntityTypeConfiguration : IEntityTypeConfiguration<CatalogBrand>
{
    public void Configure(EntityTypeBuilder<CatalogBrand> builder)
    {
        builder.ToTable("CatalogBrand", CatalogContext.DEFAULT_SCHEMA);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .UseHiLo("catalog_brand_hilo") // db tarafında id'yi arrtırmak için kullanılan bir algoritmadır. bir bir arttırıyor.
            .IsRequired();

        builder.Property(x => x.Brand)
            .IsRequired()
            .HasMaxLength(100);
    }
}
