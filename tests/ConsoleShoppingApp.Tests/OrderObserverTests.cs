using ConsoleShoppingApp.Domain.Models;
using ConsoleShoppingApp.Domain;
using ConsoleShoppingApp.Domain.Observers;
using ConsoleShoppingApp.Services;
using ConsoleShoppingApp.Data;
using ConsoleShoppingApp.Domain.Repositories;

namespace ConsoleShoppingApp.Tests;

public class OrderObserverTests
{
    private class TestObserver : IOrderObserver
    {
        public Order? NotifiedOrder { get; private set; }
        public int CallCount { get; private set; }

        public void Update(Order order)
        {
            NotifiedOrder = order;
            CallCount++;
        }
    }

    public static void Run()
    {
        Console.WriteLine("Running OrderObserverTests...");

        var store = InMemoryStore.Instance;
        var persistence = new JsonStorePersistence();
        var productRepo = new ProductRepository(store);
        var orderRepo = new OrderRepository(store);
        var catalogService = new CatalogService(productRepo, store, persistence);
        var cartService = new CartService(store, catalogService, persistence);
        var orderService = new OrderService(orderRepo, productRepo, store, catalogService, cartService, new WalletPaymentStrategy(), persistence);

        var observer = new TestObserver();
        orderService.Subscribe(observer);

        var order = new Order(Guid.NewGuid(), new List<OrderItem>(), 100m);
        orderRepo.Add(order);

        orderService.UpdateOrderStatus(order.Id, OrderStatus.Shipped);

        TestAssert.Equal(OrderStatus.Shipped, order.Status, "Order status should be updated.");
        TestAssert.Equal(1, observer.CallCount, "Observer should be called once.");
        TestAssert.Equal(order.Id, observer.NotifiedOrder?.Id, "Observer should be notified with the correct order.");

        Console.WriteLine("OrderObserverTests passed.");
    }
}
