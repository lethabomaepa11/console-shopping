using System.Text.Json;
using ConsoleShoppingApp.Domain.Models;
using ConsoleShoppingApp.Domain;

namespace ConsoleShoppingApp.Services;

public interface IStorePersistence
{
    void Load(InMemoryStore store);
    void Save(InMemoryStore store);
}

public sealed class JsonStorePersistence : IStorePersistence
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public JsonStorePersistence(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(Environment.CurrentDirectory, "Data", "store.json");
    }

    public void Load(InMemoryStore store)
    {
        if (!File.Exists(_filePath))
        {
            return;
        }

        var json = File.ReadAllText(_filePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        var snapshot = JsonSerializer.Deserialize<StoreSnapshot>(json, _jsonOptions);
        if (snapshot is null)
        {
            return;
        }

        var payments = snapshot.Payments
            .Select(p => new Payment(p.Id, p.OrderId, p.Amount, p.Method, p.Status, p.PaidAt))
            .ToList();

        var orders = snapshot.Orders
            .Select(o =>
            {
                var items = o.Items
                    .Select(i => new OrderItem(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice))
                    .ToList();
                var payment = payments.FirstOrDefault(p => p.OrderId == o.Id);
                return new Order(o.Id, o.CustomerId, items, o.TotalAmount, o.CreatedAt, o.Status, payment);
            })
            .ToList();

        var customers = snapshot.Customers
            .Select(c =>
            {
                var cartItems = c.CartItems
                    .Select(i => new CartItem(i.ProductId, i.Quantity, i.UnitPrice))
                    .ToList();
                var cart = new Cart(c.Id, cartItems);
                return new Customer(c.Id, c.Username, c.Password, c.FullName, c.WalletBalance, cart);
            })
            .ToList();

        var admins = snapshot.Administrators
            .Select(a => new Administrator(a.Id, a.Username, a.Password, a.FullName))
            .ToList();

        var products = snapshot.Products
            .Select(p => new Product(p.Id, p.Name, p.Description, p.Category, p.Price, p.StockQuantity, p.IsDeleted))
            .ToList();

        var reviews = snapshot.Reviews
            .Select(r => new Review(r.Id, r.ProductId, r.CustomerId, r.Rating, r.Comment, r.CreatedAt))
            .ToList();

        store.ReplaceAll(customers, admins, products, orders, reviews, payments);
    }

    public void Save(InMemoryStore store)
    {
        var snapshot = new StoreSnapshot
        {
            Customers = store.Customers
                .Select(c => new CustomerSnapshot
                {
                    Id = c.Id,
                    Username = c.Username,
                    Password = c.Password,
                    FullName = c.FullName,
                    WalletBalance = c.WalletBalance,
                    CartItems = c.Cart.Items
                        .Select(i => new CartItemSnapshot
                        {
                            ProductId = i.ProductId,
                            Quantity = i.Quantity,
                            UnitPrice = i.UnitPrice
                        })
                        .ToList()
                })
                .ToList(),
            Administrators = store.Administrators
                .Select(a => new AdministratorSnapshot
                {
                    Id = a.Id,
                    Username = a.Username,
                    Password = a.Password,
                    FullName = a.FullName
                })
                .ToList(),
            Products = store.Products
                .Select(p => new ProductSnapshot
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Category = p.Category,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    IsDeleted = p.IsDeleted
                })
                .ToList(),
            Orders = store.Orders
                .Select(o => new OrderSnapshot
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    TotalAmount = o.TotalAmount,
                    CreatedAt = o.CreatedAt,
                    Status = o.Status,
                    Items = o.Items
                        .Select(i => new OrderItemSnapshot
                        {
                            ProductId = i.ProductId,
                            ProductName = i.ProductName,
                            Quantity = i.Quantity,
                            UnitPrice = i.UnitPrice
                        })
                        .ToList()
                })
                .ToList(),
            Reviews = store.Reviews
                .Select(r => new ReviewSnapshot
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    CustomerId = r.CustomerId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToList(),
            Payments = store.Payments
                .Select(p => new PaymentSnapshot
                {
                    Id = p.Id,
                    OrderId = p.OrderId,
                    Amount = p.Amount,
                    Method = p.Method,
                    Status = p.Status,
                    PaidAt = p.PaidAt
                })
                .ToList()
        };

        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(snapshot, _jsonOptions);
        File.WriteAllText(_filePath, json);
    }
}

public sealed class StoreSnapshot
{
    public List<CustomerSnapshot> Customers { get; set; } = new();
    public List<AdministratorSnapshot> Administrators { get; set; } = new();
    public List<ProductSnapshot> Products { get; set; } = new();
    public List<OrderSnapshot> Orders { get; set; } = new();
    public List<ReviewSnapshot> Reviews { get; set; } = new();
    public List<PaymentSnapshot> Payments { get; set; } = new();
}

public sealed class CustomerSnapshot
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public decimal WalletBalance { get; set; }
    public List<CartItemSnapshot> CartItems { get; set; } = new();
}

public sealed class CartItemSnapshot
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public sealed class AdministratorSnapshot
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

public sealed class ProductSnapshot
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsDeleted { get; set; }
}

public sealed class OrderSnapshot
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItemSnapshot> Items { get; set; } = new();
}

public sealed class OrderItemSnapshot
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public sealed class ReviewSnapshot
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid CustomerId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class PaymentSnapshot
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime PaidAt { get; set; }
}
