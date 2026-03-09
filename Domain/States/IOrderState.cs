using ConsoleShoppingApp.Domain.Models;

namespace ConsoleShoppingApp.Domain.States;

public interface IOrderState
{
    OrderStatus Status { get; }
    bool CanTransitionTo(OrderStatus targetStatus);
}
