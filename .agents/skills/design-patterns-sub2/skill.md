---
name: design-patterns-sub2
description: Implement software design patterns and refactor architecture for Submission 2 of the Online Shopping Console App. Use this skill whenever the user asks about design patterns, Singleton, Factory, Observer, Strategy, Repository, refactoring for Submission 2, separating concerns, improving architecture, or making the codebase more maintainable. Also trigger when the user says "Submission 2", "design patterns", "refactor", "clean architecture", "SOLID", or asks how to improve code structure beyond the initial working version.
---

# Design Patterns — Submission 2

This skill refactors the Submission 1 codebase using software design patterns. Each pattern is explained with its purpose and a concrete implementation for this project.

---

## Pattern Overview

| Pattern    | Type        | Applied To                 |
| ---------- | ----------- | -------------------------- |
| Singleton  | Creational  | Central data store         |
| Factory    | Creational  | User creation              |
| Observer   | Behavioural | Order status notifications |
| Strategy   | Behavioural | Payment processing         |
| Repository | Structural  | Data access layer          |

Implement at minimum: **Singleton + Factory + one behavioural pattern** for full marks.

---

## 1. Singleton — DataStore

**Why:** Replaces scattered `static List<T>` variables in `Program.cs` with a single, globally accessible data store. Ensures only one instance of data exists.

```csharp
// Patterns/DataStore.cs
public sealed class DataStore
{
    private static DataStore _instance;
    private static readonly object _lock = new();

    public static DataStore Instance
    {
        get {
            if (_instance == null)
                lock (_lock) { _instance ??= new DataStore(); }
            return _instance;
        }
    }

    public List<User> Users { get; } = new();
    public List<Product> Products { get; } = new();
    public List<Order> Orders { get; } = new();
    public List<Payment> Payments { get; } = new();

    public int NextUserId { get; set; } = 1;
    public int NextProductId { get; set; } = 1;
    public int NextOrderId { get; set; } = 1;

    private DataStore() => SeedData();

    private void SeedData()
    {
        Products.Add(new Product { Id = NextProductId++, Name = "Laptop", Category = "Electronics", Price = 12999m, Stock = 10 });
        Products.Add(new Product { Id = NextProductId++, Name = "Headphones", Category = "Electronics", Price = 799m, Stock = 25 });
        Users.Add(new Customer("Alice", "alice@test.com", "pass123") { Id = NextUserId++, WalletBalance = 5000m });
        Users.Add(new Administrator("Admin", "admin@test.com", "admin123") { Id = NextUserId++ });
    }
}
```

**Usage:**

```csharp
// Before (Submission 1)
static List<Product> products = new();

// After (Submission 2)
var products = DataStore.Instance.Products;
```

---

## 2. Factory — UserFactory

**Why:** Centralises user creation logic. Removes the inline `if role == "admin"` branching from menus.

```csharp
// Patterns/UserFactory.cs
public static class UserFactory
{
    public static User Create(string role, string name, string email, string password)
    {
        var store = DataStore.Instance;
        return role.ToLower() switch
        {
            "customer" => new Customer(name, email, password) { Id = store.NextUserId++ },
            "admin"    => new Administrator(name, email, password) { Id = store.NextUserId++ },
            _          => throw new ArgumentException($"Unknown role: {role}")
        };
    }
}
```

**Usage:**

```csharp
// Before
User user = role == "admin"
    ? new Administrator(name, email, password) { Id = nextUserId++ }
    : new Customer(name, email, password) { Id = nextUserId++ };

// After
User user = UserFactory.Create(role, name, email, password);
```

---

## 3. Observer — Order Notifications

**Why:** Decouples order status updates from notification logic. Customers are notified automatically when their order changes.

```csharp
// Patterns/IOrderObserver.cs
public interface IOrderObserver
{
    void OnOrderStatusChanged(Order order);
}

// Patterns/OrderNotificationManager.cs
public class OrderNotificationManager
{
    private readonly List<IOrderObserver> _observers = new();

    public void Subscribe(IOrderObserver observer) => _observers.Add(observer);
    public void Unsubscribe(IOrderObserver observer) => _observers.Remove(observer);

    public void Notify(Order order)
    {
        foreach (var obs in _observers)
            obs.OnOrderStatusChanged(order);
    }
}
```

```csharp
// Customer implements the observer
public class Customer : User, IOrderObserver
{
    public void OnOrderStatusChanged(Order order)
    {
        if (order.CustomerId == this.Id)
            Console.WriteLine($"[📦 Notification] Order #{order.Id} is now: {order.Status}");
    }
    // ... rest of class
}
```

**Usage in UpdateOrderStatus:**

```csharp
static OrderNotificationManager notificationManager = new();

static void UpdateOrderStatus()
{
    // ... get order ...
    order.Status = newStatus;

    // Notify all subscribed customers
    notificationManager.Notify(order);
    Console.WriteLine($"✓ Status updated and customers notified.");
}

// On login, subscribe the customer
static void Login()
{
    // ... after verifying credentials ...
    if (user is Customer c)
    {
        notificationManager.Subscribe(c);
        ShowCustomerMenu(c);
        notificationManager.Unsubscribe(c); // cleanup on logout
    }
}
```

---

## 4. Strategy — Payment Processing

**Why:** Makes payment methods swappable without changing checkout logic. Easy to add new payment types (credit card, voucher) later.

```csharp
// Patterns/IPaymentStrategy.cs
public interface IPaymentStrategy
{
    bool ProcessPayment(decimal amount, Customer customer);
    string MethodName { get; }
}

// Patterns/WalletPaymentStrategy.cs
public class WalletPaymentStrategy : IPaymentStrategy
{
    public string MethodName => "Wallet";

    public bool ProcessPayment(decimal amount, Customer customer)
    {
        if (customer.WalletBalance < amount)
        {
            Console.WriteLine($"✗ Insufficient wallet balance (R{customer.WalletBalance:F2} < R{amount:F2})");
            return false;
        }
        customer.WalletBalance -= amount;
        return true;
    }
}
```

**Checkout uses strategy:**

```csharp
static void Checkout(Customer customer)
{
    // ...cart validation...

    IPaymentStrategy strategy = new WalletPaymentStrategy();
    bool paid = strategy.ProcessPayment(customer.Cart.Total, customer);

    if (!paid) return;

    // ...build and save order...
    Console.WriteLine($"✓ Paid via {strategy.MethodName}.");
}
```

---

## 5. Repository — Data Access Layer

**Why:** Abstracts data access behind an interface. Business logic doesn't need to know how data is stored.

```csharp
// Repositories/IRepository.cs
public interface IRepository<T>
{
    T GetById(int id);
    List<T> GetAll();
    void Add(T entity);
    void Update(T entity);
    void Delete(int id);
}

// Repositories/ProductRepository.cs
public class ProductRepository : IRepository<Product>
{
    private readonly List<Product> _products = DataStore.Instance.Products;

    public Product GetById(int id) => _products.FirstOrDefault(p => p.Id == id);
    public List<Product> GetAll() => _products.ToList();
    public void Add(Product product) => _products.Add(product);
    public void Update(Product updated)
    {
        var existing = GetById(updated.Id);
        if (existing == null) throw new KeyNotFoundException($"Product {updated.Id} not found.");
        var idx = _products.IndexOf(existing);
        _products[idx] = updated;
    }
    public void Delete(int id) => _products.RemoveAll(p => p.Id == id);

    // Domain-specific queries
    public List<Product> Search(string query) =>
        _products.Where(p => p.Matches(query)).ToList();

    public List<Product> GetLowStock(int threshold = 5) =>
        _products.Where(p => p.Stock < threshold).OrderBy(p => p.Stock).ToList();
}
```

**Usage:**

```csharp
// Before
var product = products.FirstOrDefault(p => p.Id == id);

// After
var repo = new ProductRepository();
var product = repo.GetById(id);
```

---

## Submission 2 File Structure

```
OnlineShoppingApp/
├── Program.cs
├── Models/           ← unchanged from Sub 1
├── Interfaces/       ← unchanged from Sub 1
├── Patterns/
│   ├── DataStore.cs              (Singleton)
│   ├── UserFactory.cs            (Factory)
│   ├── IOrderObserver.cs         (Observer interface)
│   └── OrderNotificationManager.cs
├── Repositories/
│   ├── IRepository.cs
│   ├── ProductRepository.cs
│   └── OrderRepository.cs
├── Services/
│   ├── AuthService.cs
│   ├── CheckoutService.cs
│   └── ReportService.cs
└── UI/
    ├── CustomerMenu.cs
    └── AdminMenu.cs
```

---

## README Template (10% of marks)

```markdown
# Online Shopping Console App — Submission 2

## Design Patterns Used

### Singleton (DataStore)

Ensures a single shared instance of all data collections, replacing
scattered static variables from Submission 1.

### Factory (UserFactory)

Centralises user creation. Adding a new role (e.g. Vendor) requires
changing only the factory, not the menu code.

### Observer (OrderNotificationManager)

Customers are notified automatically when order status changes.
Decouples the Admin menu from customer notification logic.

### Strategy (WalletPaymentStrategy)

Payment logic is behind an interface, making it trivial to add
credit card or voucher payments later without touching checkout.

### Repository (ProductRepository, OrderRepository)

Data access is behind interfaces. If a database were added, only the
repository implementations would change — not the business logic.

## Architecture Improvements

- Business logic moved from Program.cs into Services/
- Data access moved into Repositories/
- UI layer only handles input/output, no business logic
```
