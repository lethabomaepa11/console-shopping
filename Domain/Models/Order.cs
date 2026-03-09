using ConsoleShoppingApp.Domain.States;
using ConsoleShoppingApp.Domain;

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
        _state = OrderStateFactory.Create(status);
        Payment = payment;
    }

    private IOrderState _state;

    public Guid Id { get; }
    public Guid CustomerId { get; }
    public List<OrderItem> Items { get; }
    public decimal TotalAmount { get; }
    public DateTime CreatedAt { get; }
    public OrderStatus Status => _state.Status;
    public Payment? Payment { get; set; }

    public void TransitionTo(OrderStatus nextStatus)
    {
        if (Status == nextStatus)
        {
            return;
        }

        if (!_state.CanTransitionTo(nextStatus))
        {
            throw new DomainException($"Invalid order status transition: {Status} -> {nextStatus}.");
        }

        _state = OrderStateFactory.Create(nextStatus);
    }
}
