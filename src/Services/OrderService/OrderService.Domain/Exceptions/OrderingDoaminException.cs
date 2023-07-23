namespace OrderService.Domain;

public class OrderingDoaminException : Exception
{
    public OrderingDoaminException() { }

    public OrderingDoaminException(string message) : base(message) { }

    public OrderingDoaminException(string message,  Exception innerException) : base(message, innerException) { }
}
