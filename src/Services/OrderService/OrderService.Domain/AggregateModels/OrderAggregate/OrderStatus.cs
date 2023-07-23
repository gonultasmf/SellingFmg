namespace OrderService.Domain;

public class OrderStatus : Enumeration
{
    public static OrderStatus Submitted = new(1, nameof(Submitted).ToLowerInvariant());
    public static OrderStatus AwaitingValidation = new(2, nameof(AwaitingValidation).ToLowerInvariant());
    public static OrderStatus StockConfireed = new(3, nameof(StockConfireed).ToLowerInvariant());
    public static OrderStatus Paid = new(4, nameof(Paid).ToLowerInvariant());
    public static OrderStatus Shipped = new(5, nameof(Shipped).ToLowerInvariant());
    public static OrderStatus Cancelled = new(6, nameof(Cancelled).ToLowerInvariant());

    public OrderStatus(int id, string name) : base(id, name)
    {
    }

    public static IEnumerable<OrderStatus> List() =>
        new[] { Submitted, AwaitingValidation, StockConfireed, Paid, Shipped, Cancelled };

    public static OrderStatus FromName(string name)
    {
        var state = List()
            .SingleOrDefault(x => String.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));

        return state ?? throw new OrderingDoaminException($"Possible values for OrderStatus: {String.Join(",", List().Select(x => x.Name))}");
    }

    public static OrderStatus From(int id)
    {
        var state = List() .SingleOrDefault( x => x.Id == id);

        return state ?? throw new OrderingDoaminException($"Possible values for OrderStatus: {String.Join(",", List().Select(x => x.Name))}");
    }
}
