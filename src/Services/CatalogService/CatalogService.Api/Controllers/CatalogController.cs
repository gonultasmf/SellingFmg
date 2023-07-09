using CatalogService.Application;
using CatalogService.Domain;
using CatalogService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;

namespace CatalogService.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CatalogController : ControllerBase
{
    private readonly CatalogContext _catalogContext;
    private readonly CatalogSettings _settings;

    public CatalogController(CatalogContext catalogContext, IOptionsSnapshot<CatalogSettings> settings)
    {
        _catalogContext = catalogContext;
        _settings = settings.Value;

        catalogContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    [HttpGet]
    [Route("items")]
    [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(IEnumerable<CatalogItem>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ItemsAsync([FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0, string? ids = null)
    {
        if (!string.IsNullOrEmpty(ids))
        {
            var items = await GetItemsByIdsAsync(ids);
            if (!items.Any())
            {
                return BadRequest("ids value invalid. Must be comma-separated list of members");
            }

            return Ok(items);
        }

        var totalItems = await _catalogContext.CatalogItems.LongCountAsync();
        var itemsOnPage = await _catalogContext.CatalogItems
            .OrderBy(x => x.Name)
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        itemsOnPage = ChangeUriPlaceholder(itemsOnPage);

        var model = new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);

        return Ok(model);
    }

    [HttpGet]
    [Route("items/{id:int}")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(CatalogItem), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<CatalogItem>> ItemByIdAsync(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var item = await _catalogContext.CatalogItems.SingleOrDefaultAsync(x => x.Id == id);

        var baseUri = _settings.PicBaseUrl;

        if (item is not null)
        {
            item.PictureUri = baseUri + item.PictureFileName;

            return item;
        }

        return NotFound();
    }

    [HttpGet]
    [Route("items/withname/{name:minlength(1)}")]
    [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<PaginatedItemsViewModel<CatalogItem>>> ItemWithNameAsync(string name, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
    {
        var totalItems = await _catalogContext.CatalogItems
            .Where(x => x.Name.StartsWith(name))
            .LongCountAsync();

        var itemsOnPage = await _catalogContext.CatalogItems
            .Where(x => x.Name.StartsWith(name))
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        itemsOnPage = ChangeUriPlaceholder(itemsOnPage);

        return new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);
    }

    [HttpGet]
    [Route("items/type/{catalogTypeId}/brand/{catalogBrandId:int}")]
    [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<PaginatedItemsViewModel<CatalogItem>>> ItemsByTypeIdAndBrandIdAsync(int catalogTypeId, int? catalogBrandId, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
    {
        var root = (IQueryable<CatalogItem>)_catalogContext.CatalogItems;

        root = root.Where(x => x.CatalogTypeId == catalogTypeId);

        if (catalogBrandId.HasValue)
        {
            root = root.Where(x => x.CatalogBrandId == catalogBrandId);
        }

        var totalItems = await root.LongCountAsync();

        var itemsOnPage = await root
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        itemsOnPage = ChangeUriPlaceholder(itemsOnPage);

        return new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);
    }

    [HttpGet]
    [Route("items/type/all/brand/{catalogBrandId:int}")]
    [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<PaginatedItemsViewModel<CatalogItem>>> ItemsByBrandIdAsync(int? catalogBrandId, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
    {
        var root = (IQueryable<CatalogItem>)_catalogContext.CatalogItems;

        if (catalogBrandId.HasValue)
        {
            root = root.Where(x => x.CatalogBrandId == catalogBrandId);
        }

        var totalItems = await root.LongCountAsync();

        var itemsOnPage = await root
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        itemsOnPage = ChangeUriPlaceholder(itemsOnPage);

        return new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);
    }

    [HttpGet]
    [Route("catalogTypes")]
    [ProducesResponseType(typeof(List<CatalogItem>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<List<CatalogType>>> CatalogTypesAsync()
    {
        return await _catalogContext.CatalogTypes.ToListAsync();
    }

    [HttpGet]
    [Route("catalogBrands")]
    [ProducesResponseType(typeof(List<CatalogBrand>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<List<CatalogBrand>>> CatalogBrandsAsync()
    {
        return await _catalogContext.CatalogBrands.ToListAsync();
    }

    [HttpPut]
    [Route("items")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<ActionResult<CatalogItem>> UpdateProductAsync([FromBody] CatalogItem productToUpdate)
    {
        var catalogItem = await _catalogContext.CatalogItems.SingleOrDefaultAsync(x => x.Id == productToUpdate.Id);

        if (catalogItem is null)
        {
            return NotFound(new { message = $"Item with id {productToUpdate.Id} not found." });
        }

        var oldPrice = catalogItem.Price;
        var raiseProductPriceChangeEvent = oldPrice + productToUpdate.Price;

        catalogItem = productToUpdate;

        _catalogContext.CatalogItems.Update(catalogItem);
        await _catalogContext.SaveChangesAsync();

        return CreatedAtAction(nameof(ItemByIdAsync), new { id = productToUpdate.Id }, null);
    }

    [HttpPost]
    [Route("items")]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<ActionResult<CatalogItem>> CreateProductAsync([FromBody] CatalogItem product)
    {
        var item = new CatalogItem
        {
            CatalogBrandId = product.CatalogBrandId,
            CatalogTypeId = product.CatalogTypeId,
            Description = product.Description,
            Name = product.Name,
            PictureFileName = product.PictureFileName,
            Price = product.Price
        };

        _catalogContext.CatalogItems.Add(item);
        await _catalogContext.SaveChangesAsync();

        return CreatedAtAction(nameof(ItemByIdAsync), new { id = item.Id }, null);
    }

    [HttpDelete]
    [Route("{id}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> DeleteProductAsync(int id)
    {
        var product = await _catalogContext.CatalogItems.SingleOrDefaultAsync(x => x.Id == id);

        if (product is null) return NotFound();

        _catalogContext.CatalogItems.Remove(product);
        await _catalogContext.SaveChangesAsync();

        return NoContent();
    }


    private List<CatalogItem> ChangeUriPlaceholder(List<CatalogItem> items)
    {
        var baseUri = _settings.PicBaseUrl;

        foreach (var item in items)
        {
            if (item is not null)
                item.PictureUri = baseUri + item.PictureFileName;
        }

        return items;
    }

    private async Task<List<CatalogItem>> GetItemsByIdsAsync(string ids)
    {
        var numIds = ids.Split(',').Select(id => (Ok: int.TryParse(id, out int x), Value: x));

        if (!numIds.All(nid => nid.Ok))
        {
            return new List<CatalogItem>();
        }

        var idsToSelect = numIds.Select(id => id.Value);

        var items = await _catalogContext.CatalogItems.Where(ci => idsToSelect.Contains(ci.Id)).ToListAsync();

        items = ChangeUriPlaceholder(items);

        return items;
    }
}
