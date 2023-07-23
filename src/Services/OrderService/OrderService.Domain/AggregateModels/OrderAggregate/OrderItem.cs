using System.ComponentModel.DataAnnotations;

namespace OrderService.Domain;

public class OrderItem : BaseEntity, IValidatableObject
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string PictureUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Units { get; set; }

    protected OrderItem()
    {
    }

    public OrderItem(int productId, string productName, string pictureUrl, decimal unitPrice, int units)
    {
        ProductId = productId;
        ProductName = productName;
        PictureUrl = pictureUrl;
        UnitPrice = unitPrice;
        Units = units;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var result = new List<ValidationResult>();

        if (Units <= 0)
            result.Add(new ValidationResult("Invalid number of units", new[] { "Units" }));

        return result;
    }
}
