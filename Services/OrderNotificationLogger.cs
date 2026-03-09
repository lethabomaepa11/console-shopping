using ConsoleShoppingApp.Domain.Models;
using ConsoleShoppingApp.Domain;
using ConsoleShoppingApp.Domain.Observers;

namespace ConsoleShoppingApp.Services;

public class OrderNotificationLogger : IOrderObserver
{
    public void Update(Order order)
    {
        Console.WriteLine($"[NOTIFICATION] Order {order.Id} status updated to: {order.Status}");
    }
}
