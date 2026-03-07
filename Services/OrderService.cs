using ConsoleShoppingApp.Domain;

namespace ConsoleShoppingApp.Services;

public sealed class OrderService
{
    private readonly InMemoryStore _store;
    private readonly CatalogService _catalogService;
    private readonly CartService _cartService;
    private readonly IPaymentStrategy _paymentStrategy;
    private readonly IStorePersistence _persistence;

    public OrderService(
        InMemoryStore store,
        CatalogService catalogService,
        CartService cartService,
        IPaymentStrategy paymentStrategy,
        IStorePersistence persistence)
    {
        _store = store;
        _catalogService = catalogService;
        _cartService = cartService;
        _paymentStrategy = paymentStrategy;
        _persistence = persistence;
    }

    public Order Checkout(Guid customerId)
    {
        var customer = _cartService.GetCustomerOrThrow(customerId);
        if (!customer.Cart.Items.Any())
        {
            throw new DomainException("Cart is empty.");
        }

        var orderItems = new List<OrderItem>();
        foreach (var cartItem in customer.Cart.Items)
        {
            var product = _catalogService.GetProductOrThrow(cartItem.ProductId);
            if (product.IsDeleted || product.StockQuantity < cartItem.Quantity)
            {
                throw new DomainException($"Insufficient stock for product: {product.Name}");
            }

            orderItems.Add(new OrderItem(product.Id, product.Name, cartItem.Quantity, product.Price));
        }

        var total = orderItems.Sum(item => item.Quantity * item.UnitPrice);
        var order = new Order(customerId, orderItems, total);
        var payment = _paymentStrategy.ProcessPayment(customer, order);
        order.Payment = payment;

        if (payment.Status != PaymentStatus.Completed)
        {
            throw new DomainException("Payment failed. Add wallet funds and retry.");
        }

        foreach (var item in orderItems)
        {
            var product = _catalogService.GetProductOrThrow(item.ProductId);
            product.StockQuantity -= item.Quantity;
        }

        order.Status = OrderStatus.Paid;
        _store.Orders.Add(order);
        _store.Payments.Add(payment);
        customer.Cart.Items.Clear();
        _persistence.Save(_store);
        return order;
    }

    public List<Order> GetCustomerOrders(Guid customerId)
    {
        return _store.Orders
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToList();
    }

    public List<Order> GetAllOrders()
    {
        return _store.Orders
            .OrderByDescending(o => o.CreatedAt)
            .ToList();
    }

    public void UpdateOrderStatus(Guid orderId, OrderStatus newStatus)
    {
        var order = _store.Orders.FirstOrDefault(o => o.Id == orderId);
        if (order is null)
        {
            throw new DomainException("Order not found.");
        }

        order.Status = newStatus;
        _persistence.Save(_store);
    }

    public bool HasCustomerPurchasedProduct(Guid customerId, Guid productId)
    {
        return _store.Orders.Any(o =>
            o.CustomerId == customerId &&
            o.Status != OrderStatus.Cancelled &&
            o.Items.Any(i => i.ProductId == productId));
    }
}
