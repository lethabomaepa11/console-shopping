using ConsoleShoppingApp.Domain.Models;

namespace ConsoleShoppingApp.Domain;

public sealed class InMemoryStore
{
    private static readonly Lazy<InMemoryStore> LazyInstance = new(() => new InMemoryStore());

    private InMemoryStore()
    {
    }

    public static InMemoryStore Instance => LazyInstance.Value;

    public List<Customer> Customers { get; } = new();
    public List<Administrator> Administrators { get; } = new();
    public List<Product> Products { get; } = new();
    public List<Order> Orders { get; } = new();
    public List<Review> Reviews { get; } = new();
    public List<Payment> Payments { get; } = new();

    public void ReplaceAll(
        IEnumerable<Customer> customers,
        IEnumerable<Administrator> administrators,
        IEnumerable<Product> products,
        IEnumerable<Order> orders,
        IEnumerable<Review> reviews,
        IEnumerable<Payment> payments)
    {
        Customers.Clear();
        Customers.AddRange(customers);
        Administrators.Clear();
        Administrators.AddRange(administrators);
        Products.Clear();
        Products.AddRange(products);
        Orders.Clear();
        Orders.AddRange(orders);
        Reviews.Clear();
        Reviews.AddRange(reviews);
        Payments.Clear();
        Payments.AddRange(payments);
    }
}
