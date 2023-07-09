using CatalogService.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Polly;
using System.Globalization;
using System.IO.Compression;

namespace CatalogService.Infrastructure;

public class CatalogContextSeed
{
	public async Task SeedAsync(CatalogContext context, ILogger<CatalogContextSeed> logger)
	{
		var policy = Policy.Handle<SqlException>()
			.WaitAndRetryAsync(
				retryCount: 3, 
				sleepDurationProvider: 
				retry =>  TimeSpan.FromSeconds(5),
				onRetry:(exception, timeSpan, retry, ctx) =>
				{
                    logger.LogWarning(exception, "[{prefix}] Exception {ExceptionType} with message {Message} detected on attemp {retry} of {retry}");
				});

		var setupDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Setup", "SeedFiles");
		var picturePath = "Pics";

		await policy.ExecuteAsync(() => ProcessSeeding(context, setupDirPath, picturePath, logger));
	}

    private async Task ProcessSeeding(CatalogContext context, string setupDirPath, string picturePath, ILogger<CatalogContextSeed> logger)
    {
        if (!context.CatalogBrands.Any())
        {
			await context.CatalogBrands.AddRangeAsync(GetCatalogBrandsFromFile(setupDirPath));
            await context.SaveChangesAsync();
        }

        if (!context.CatalogTypes.Any())
        {
			await context.CatalogTypes.AddRangeAsync(GetCatalogTypesFromFile(setupDirPath));
            await context.SaveChangesAsync();
        }

        if (!context.CatalogItems.Any())
        {
			await context.CatalogItems.AddRangeAsync(GetCatalogItemsFromFile(setupDirPath, context));
            await context.SaveChangesAsync();

            //GetCatalogItemsPictures(setupDirPath, picturePath);
        }
    }

    private IEnumerable<CatalogBrand> GetCatalogBrandsFromFile(string contentPath)
    {
        IEnumerable<CatalogBrand> GetPreconfiguredCatalogBrand()
        {
            return new List<CatalogBrand>()
            {
                new CatalogBrand { Brand = "Azure" },
                new CatalogBrand { Brand = ".NET" },
                new CatalogBrand { Brand = "Visual Studio" },
                new CatalogBrand { Brand = "SQL Server" },
                new CatalogBrand { Brand = "Other" }
            };
        }

        string fileName = Path.Combine(contentPath, "BrandsTextFile.txt");

        if (!File.Exists(fileName))
        {
            return GetPreconfiguredCatalogBrand();
        }

        var fileContent = File.ReadAllLines(fileName);

        var list = fileContent.Select(x => new CatalogBrand
        {
            Brand = x,
        }).Where(x => x is not null);

        return list ?? GetPreconfiguredCatalogBrand();
    }

    private IEnumerable<CatalogType> GetCatalogTypesFromFile(string contentPath)
    {
        IEnumerable<CatalogType> GetPreconfiguredCatalogTypes()
        {
            return new List<CatalogType>()
            {
                new CatalogType { Type = "Mug" },
                new CatalogType { Type = "T-Shirt" },
                new CatalogType { Type = "Sheet" },
                new CatalogType { Type = "USB Memory Stick" }
            };
        }

        string fileName = Path.Combine(contentPath, "TypesTextFile.txt");

        if (!File.Exists(fileName))
        {
            return GetPreconfiguredCatalogTypes();
        }

        var fileContent = File.ReadAllLines(fileName);

        var list = fileContent.Select(x => new CatalogType
        {
            Type = x,
        }).Where(x => x is not  null);

        return list ?? GetPreconfiguredCatalogTypes();
    }

    private IEnumerable<CatalogItem> GetCatalogItemsFromFile(string contentPath, CatalogContext context)
    {
        IEnumerable<CatalogItem> GetPreconfiguredItems()
        {
            return new List<CatalogItem>()
            {
                new CatalogItem { CatalogTypeId = 2, CatalogBrandId = 2, AvailableStock = 100, Description = ".NET Black & White Mug", Name = "Catalog Name 1", OnReorder = false, PictureFileName = "1.png", Price = 37.5M },
                new CatalogItem { CatalogTypeId = 1, CatalogBrandId = 5, AvailableStock = 100, Description = ".NET Black & White Mug", Name = "Catalog Name 2", OnReorder = false, PictureFileName = "2.png", Price = 37.5M },
                new CatalogItem { CatalogTypeId = 2, CatalogBrandId = 5, AvailableStock = 100, Description = ".NET Black & White Mug", Name = "Catalog Name 3", OnReorder = false, PictureFileName = "3.png", Price = 37.5M },
                new CatalogItem { CatalogTypeId = 2, CatalogBrandId = 2, AvailableStock = 100, Description = ".NET Black & White Mug", Name = "Catalog Name 4", OnReorder = false, PictureFileName = "4.png", Price = 37.5M },
                new CatalogItem { CatalogTypeId = 2, CatalogBrandId = 5, AvailableStock = 100, Description = ".NET Black & White Mug", Name = "Catalog Name 5", OnReorder = false, PictureFileName = "5.png", Price = 37.5M },
                new CatalogItem { CatalogTypeId = 1, CatalogBrandId = 5, AvailableStock = 100, Description = ".NET Black & White Mug", Name = "Catalog Name 6", OnReorder = false, PictureFileName = "6.png", Price = 37.5M },
                new CatalogItem { CatalogTypeId = 1, CatalogBrandId = 2, AvailableStock = 100, Description = ".NET Black & White Mug", Name = "Catalog Name 7", OnReorder = false, PictureFileName = "7.png", Price = 37.5M },
                new CatalogItem { CatalogTypeId = 2, CatalogBrandId = 2, AvailableStock = 100, Description = ".NET Black & White Mug", Name = "Catalog Name 8", OnReorder = false, PictureFileName = "8.png", Price = 37.5M },
                new CatalogItem { CatalogTypeId = 1, CatalogBrandId = 5, AvailableStock = 100, Description = ".NET Black & White Mug", Name = "Catalog Name 9", OnReorder = false, PictureFileName = "9.png", Price = 37.5M },
                new CatalogItem { CatalogTypeId = 1, CatalogBrandId = 2, AvailableStock = 100, Description = ".NET Black & White Mug", Name = "Catalog Name 10", OnReorder = false, PictureFileName = "10.png", Price = 37.5M },
                new CatalogItem { CatalogTypeId = 2, CatalogBrandId = 5, AvailableStock = 100, Description = ".NET Black & White Mug", Name = "Catalog Name 11", OnReorder = false, PictureFileName = "11.png", Price = 37.5M }
            };
        }

        string fileName = Path.Combine(contentPath, "CatalogsTextFile");

        if (!File.Exists(fileName))
        {
            return GetPreconfiguredItems();
        }

        var catalogTypeIdLookup = context.CatalogTypes.ToDictionary(x => x.Type, x => x.Id);
        var catalogBrandIdLookup = context.CatalogBrands.ToDictionary(x => x.Brand, x => x.Id);

        var fileContent = File.ReadAllLines(fileName)
            .Skip(1) // header satırı atlanıldı.
            .Select(i => i.Split(','))
            .Select(i => new CatalogItem
            {
                CatalogTypeId = catalogTypeIdLookup[i[0]],
                CatalogBrandId = catalogBrandIdLookup[i[1]],
                Description = i[2].Trim('"').Trim(),
                Name = i[3].Trim('"').Trim(),
                Price = Decimal.Parse(i[4].Trim('"').Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
                PictureFileName = i[5].Trim('"').Trim(),
                AvailableStock = string.IsNullOrWhiteSpace(i[6]) ? 0 : int.Parse(i[6]),
                OnReorder = Convert.ToBoolean(i[7])
            });

        return fileContent;
    }

    private void GetCatalogItemsPictures(string contentPath, string picturePath)
    {
        picturePath ??= "pics";

        if (picturePath is not null)
        {
            DirectoryInfo directory = new DirectoryInfo(picturePath);
            foreach (var file in directory.GetFiles())
            {
                file.Delete();
            }

            string zipFileCatalogItemPictures = Path.Combine(contentPath, "CatalogItems.zip");
            ZipFile.ExtractToDirectory(zipFileCatalogItemPictures, picturePath);
        }
    }
}
