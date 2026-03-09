using ConsoleShoppingApp.Domain;
using ConsoleShoppingApp.Domain.Repositories;
using ConsoleShoppingApp.Domain.Observers;

namespace ConsoleShoppingApp.Services;

public sealed class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly InMemoryStore _store;
    private readonly CatalogService _catalogService;
    private readonly CartService _cartService;
    private readonly IPaymentStrategy _paymentStrategy;
    private readonly IStorePersistence _persistence;
    private readonly List<IOrderObserver> _observers = new();

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        InMemoryStore store,
        CatalogService catalogService,
        CartService cartService,
        IPaymentStrategy paymentStrategy,
        IStorePersistence persistence)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _store = store;
        _catalogService = catalogService;
        _cartService = cartService;
        _paymentStrategy = paymentStrategy;
        _persistence = persistence;
    }

    public void Subscribe(IOrderObserver observer) => _observers.Add(observer);
    public void Unsubscribe(IOrderObserver observer) => _observers.Remove(observer);

    public void Notify(Order order)
    {
        foreach (var observer in _observers)
        {
            observer.Update(order);
        }
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
            _productRepository.Update(product);
        }

        order.Status = OrderStatus.Paid;
        _orderRepository.Add(order);
        _store.Payments.Add(payment);
        customer.Cart.Items.Clear();
        _persistence.Save(_store);

        Notify(order);

        return order;
    }

    public List<Order> GetCustomerOrders(Guid customerId)
    {
        return _orderRepository.GetByCustomerId(customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToList();
    }

    public List<Order> GetAllOrders()
    {
        return _orderRepository.GetAll()
            .OrderByDescending(o => o.CreatedAt)
            .ToList();
    }

    public void UpdateOrderStatus(Guid orderId, OrderStatus newStatus)
    {
        var order = _orderRepository.GetById(orderId);
        if (order is null)
        {
            throw new DomainException("Order not found.");
        }

        order.Status = newStatus;
        _orderRepository.Update(order);
        _persistence.Save(_store);

        Notify(order);
    }

    public bool HasCustomerPurchasedProduct(Guid customerId, Guid productId)
    {
        return _orderRepository.HasPurchasedProduct(customerId, productId);
    }
}
