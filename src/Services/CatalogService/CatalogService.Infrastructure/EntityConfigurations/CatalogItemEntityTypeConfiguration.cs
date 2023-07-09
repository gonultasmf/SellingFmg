﻿using CatalogService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Infrastructure;

public class CatalogItemEntityTypeConfiguration : IEntityTypeConfiguration<CatalogItem>
{
    public void Configure(EntityTypeBuilder<CatalogItem> builder)
    {
        builder.ToTable("Catalog", CatalogContext.DEFAULT_SCHEMA);

        builder.Property(x => x.Id)
            .UseHiLo("catalog_hilo") // db tarafında id'yi arrtırmak için kullanılan bir algoritmadır. bir bir arttırıyor.
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Price)
            .IsRequired();

        builder.Property(x => x.PictureFileName)
            .IsRequired(false);

        builder.Ignore(x => x.PictureUri);

        builder.HasOne(x => x.CatalogBrand)
            .WithMany()
            .HasForeignKey(x => x.CatalogBrandId);

        builder.HasOne(x => x.CatalogType)
            .WithMany()
            .HasForeignKey(x => x.CatalogTypeId);
    }
}
