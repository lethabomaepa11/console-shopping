---
name: oop-domain-models
description: Define, scaffold, and fix the core C# domain classes for the Online Shopping Console App. Use this skill whenever the user asks to create or fix User, Customer, Administrator, Product, Cart, CartItem, Order, OrderItem, Payment, or Review classes. Also trigger when the user asks about inheritance, interfaces (ISearchable, IReportable, IPayable), polymorphism, class properties, constructors, or the Models/ folder. If the user says "build the domain", "scaffold the classes", or "set up the models", always use this skill.
---

# OOP Domain Models — Online Shopping Console App

This skill covers building all domain classes with correct C# OOP conventions: inheritance, interfaces, encapsulation, and polymorphism.

---

## Inheritance Hierarchy

```
User (abstract base)
├── Customer : User
└── Administrator : User
```

---

## Base Class: User

```csharp
public abstract class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    protected string Password { get; set; }
    public DateTime RegisteredAt { get; set; }

    protected User(string name, string email, string password)
    {
        Name = name;
        Email = email;
        Password = password;
        RegisteredAt = DateTime.Now;
    }

    public bool VerifyPassword(string input) => Password == input;

    public abstract void DisplayDashboard();
}
```

---

## Customer : User

```csharp
public class Customer : User
{
    public decimal WalletBalance { get; set; }
    public Cart Cart { get; set; }
    public List<Order> Orders { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();

    public Customer(string name, string email, string password)
        : base(name, email, password)
    {
        WalletBalance = 0m;
        Cart = new Cart(this);
    }

    public override void DisplayDashboard()
        => Console.WriteLine($"Welcome, {Name}! Wallet: R{WalletBalance:F2}");
}
```

---

## Administrator : User

```csharp
public class Administrator : User
{
    public Administrator(string name, string email, string password)
        : base(name, email, password) { }

    public override void DisplayDashboard()
        => Console.WriteLine($"Admin Panel — {Name}");
}
```

---

## Product

```csharp
public class Product : ISearchable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public double AverageRating { get; private set; }
    public List<Review> Reviews { get; set; } = new();

    public void AddReview(Review review)
    {
        Reviews.Add(review);
        AverageRating = Reviews.Average(r => r.Rating);
    }

    public bool Matches(string query) =>
        Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
        Category.Contains(query, StringComparison.OrdinalIgnoreCase);

    public void Display()
        => Console.WriteLine($"[{Id}] {Name} | R{Price:F2} | Stock: {Stock} | ★ {AverageRating:F1}");
}
```

---

## Cart and CartItem

```csharp
public class CartItem
{
    public Product Product { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal => Product.Price * Quantity;
}

public class Cart
{
    public Customer Owner { get; }
    public List<CartItem> Items { get; } = new();
    public decimal Total => Items.Sum(i => i.Subtotal);

    public Cart(Customer owner) => Owner = owner;

    public void AddItem(Product product, int quantity)
    {
        var existing = Items.FirstOrDefault(i => i.Product.Id == product.Id);
        if (existing != null)
            existing.Quantity += quantity;
        else
            Items.Add(new CartItem { Product = product, Quantity = quantity });
    }

    public void RemoveItem(int productId)
        => Items.RemoveAll(i => i.Product.Id == productId);

    public void Clear() => Items.Clear();

    public void Display()
    {
        if (!Items.Any()) { Console.WriteLine("Cart is empty."); return; }
        foreach (var item in Items)
            Console.WriteLine($"  {item.Product.Name} x{item.Quantity} = R{item.Subtotal:F2}");
        Console.WriteLine($"  Total: R{Total:F2}");
    }
}
```

---

## Order and OrderItem

```csharp
public enum OrderStatus { Pending, Processing, Shipped, Delivered, Cancelled }

public class OrderItem
{
    public Product Product { get; set; }
    public int Quantity { get; set; }
    public decimal PriceAtPurchase { get; set; }
    public decimal Subtotal => PriceAtPurchase * Quantity;
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.Subtotal);
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime PlacedAt { get; set; } = DateTime.Now;

    public void Display()
    {
        Console.WriteLine($"Order #{Id} | {Status} | R{Total:F2} | {PlacedAt:dd MMM yyyy}");
        foreach (var item in Items)
            Console.WriteLine($"  - {item.Product.Name} x{item.Quantity}");
    }
}
```

---

## Payment

```csharp
public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.Now;
    public bool Success { get; set; }
}
```

---

## Review

```csharp
public class Review
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public int Rating { get; set; }       // 1–5
    public string Comment { get; set; }
    public DateTime ReviewedAt { get; set; } = DateTime.Now;

    public void Display()
        => Console.WriteLine($"  ★{Rating} by {CustomerName}: \"{Comment}\"");
}
```

---

## Interfaces

```csharp
// Interfaces/ISearchable.cs
public interface ISearchable
{
    bool Matches(string query);
}

// Interfaces/IReportable.cs
public interface IReportable
{
    void GenerateReport();
}

// Interfaces/IPayable.cs
public interface IPayable
{
    bool ProcessPayment(decimal amount, Customer customer);
}
```

---

## Checklist

- [ ] All classes in `Models/` folder
- [ ] Interfaces in `Interfaces/` folder
- [ ] `User` is abstract with `DisplayDashboard()` override in both subclasses
- [ ] `Cart` belongs to `Customer`, not standalone
- [ ] `OrderItem` captures `PriceAtPurchase` (price can change later)
- [ ] `Product.AddReview()` recalculates `AverageRating`
- [ ] `OrderStatus` is an `enum`
