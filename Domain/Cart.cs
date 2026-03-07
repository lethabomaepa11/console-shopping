namespace ConsoleShoppingApp.Domain;

public sealed class Cart
{
    public Cart(Guid customerId)
        : this(customerId, new List<CartItem>())
    {
    }

    public Cart(Guid customerId, List<CartItem> items)
    {
        CustomerId = customerId;
        Items = items;
    }

    public Guid CustomerId { get; }
    public List<CartItem> Items { get; }
}
