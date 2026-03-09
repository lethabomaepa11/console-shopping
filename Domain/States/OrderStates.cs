using ConsoleShoppingApp.Domain.Models;

namespace ConsoleShoppingApp.Domain.States;

public sealed class PendingOrderState : IOrderState
{
    public OrderStatus Status => OrderStatus.Pending;

    public bool CanTransitionTo(OrderStatus targetStatus)
    {
        return targetStatus is OrderStatus.Paid or OrderStatus.Cancelled;
    }
}

public sealed class PaidOrderState : IOrderState
{
    public OrderStatus Status => OrderStatus.Paid;

    public bool CanTransitionTo(OrderStatus targetStatus)
    {
        return targetStatus is OrderStatus.Processing or OrderStatus.Cancelled;
    }
}

public sealed class ProcessingOrderState : IOrderState
{
    public OrderStatus Status => OrderStatus.Processing;

    public bool CanTransitionTo(OrderStatus targetStatus)
    {
        return targetStatus is OrderStatus.Shipped or OrderStatus.Cancelled;
    }
}

public sealed class ShippedOrderState : IOrderState
{
    public OrderStatus Status => OrderStatus.Shipped;

    public bool CanTransitionTo(OrderStatus targetStatus)
    {
        return targetStatus == OrderStatus.Delivered;
    }
}

public sealed class DeliveredOrderState : IOrderState
{
    public OrderStatus Status => OrderStatus.Delivered;

    public bool CanTransitionTo(OrderStatus targetStatus)
    {
        return false;
    }
}

public sealed class CancelledOrderState : IOrderState
{
    public OrderStatus Status => OrderStatus.Cancelled;

    public bool CanTransitionTo(OrderStatus targetStatus)
    {
        return false;
    }
}
