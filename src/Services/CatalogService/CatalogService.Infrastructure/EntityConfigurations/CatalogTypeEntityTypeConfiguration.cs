using CatalogService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Infrastructure;

public class CatalogTypeEntityTypeConfiguration : IEntityTypeConfiguration<CatalogType>
{
    public void Configure(EntityTypeBuilder<CatalogType> builder)
    {
        builder.ToTable("CatalogType", CatalogContext.DEFAULT_SCHEMA);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .UseHiLo("catalog_type_hilo") // db tarafında id'yi arrtırmak için kullanılan bir algoritmadır. bir bir arttırıyor.
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(100);
    }
}
