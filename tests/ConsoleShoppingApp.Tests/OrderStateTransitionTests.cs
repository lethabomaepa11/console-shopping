using ConsoleShoppingApp.Domain.Models;
using ConsoleShoppingApp.Domain;

namespace ConsoleShoppingApp.Tests;

internal static class OrderStateTransitionTests
{
    public static void Run()
    {
        Order_Allows_Valid_State_Progression();
        Order_Rejects_Invalid_State_Transition();
    }

    private static void Order_Allows_Valid_State_Progression()
    {
        var order = new Order(Guid.NewGuid(), new List<OrderItem>(), 100m);

        order.TransitionTo(OrderStatus.Paid);
        order.TransitionTo(OrderStatus.Processing);
        order.TransitionTo(OrderStatus.Shipped);
        order.TransitionTo(OrderStatus.Delivered);

        TestAssert.Equal(OrderStatus.Delivered, order.Status, "Order should reach delivered state through valid transitions.");
    }

    private static void Order_Rejects_Invalid_State_Transition()
    {
        var order = new Order(Guid.NewGuid(), new List<OrderItem>(), 100m);

        TestAssert.Throws<DomainException>(
            () => order.TransitionTo(OrderStatus.Shipped),
            "Invalid order status transition: Pending -> Shipped.");
    }
}
