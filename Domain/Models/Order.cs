namespace ConsoleShoppingApp.Domain.Models;

public sealed class Order
{
    public Order(Guid customerId, List<OrderItem> items, decimal totalAmount)
        : this(Guid.NewGuid(), customerId, items, totalAmount, DateTime.UtcNow, OrderStatus.Pending, null)
    {
    }

    public Order(
        Guid id,
        Guid customerId,
        List<OrderItem> items,
        decimal totalAmount,
        DateTime createdAt,
        OrderStatus status,
        Payment? payment)
    {
        Id = id;
        CustomerId = customerId;
        Items = items;
        TotalAmount = totalAmount;
        CreatedAt = createdAt;
        Status = status;
        Payment = payment;
    }

    public Guid Id { get; }
    public Guid CustomerId { get; }
    public List<OrderItem> Items { get; }
    public decimal TotalAmount { get; }
    public DateTime CreatedAt { get; }
    public OrderStatus Status { get; set; }
    public Payment? Payment { get; set; }
}
