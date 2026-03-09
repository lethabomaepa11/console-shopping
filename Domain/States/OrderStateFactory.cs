using ConsoleShoppingApp.Domain.Models;

namespace ConsoleShoppingApp.Domain.States;

public static class OrderStateFactory
{
    public static IOrderState Create(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => new PendingOrderState(),
            OrderStatus.Paid => new PaidOrderState(),
            OrderStatus.Processing => new ProcessingOrderState(),
            OrderStatus.Shipped => new ShippedOrderState(),
            OrderStatus.Delivered => new DeliveredOrderState(),
            OrderStatus.Cancelled => new CancelledOrderState(),
            _ => throw new DomainException("Unsupported order status.")
        };
    }
}
