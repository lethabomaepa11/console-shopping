---
name: linq-and-data
description: Implement all LINQ queries, business logic operations, and data manipulation for the Online Shopping Console App. Use this skill whenever the user asks to implement checkout, cart-to-order conversion, wallet payments, inventory updates, admin reports, sales analytics, product search, order filtering, low-stock alerts, or any operation that reads/writes the in-memory data collections. Trigger on phrases like "implement checkout", "generate report", "search products", "filter orders", "calculate totals", "low stock", "sales report", or any feature that requires querying List<T> collections.
---

# LINQ & Data Operations — Online Shopping Console App

This skill covers all business logic that operates on the in-memory data collections: cart/order processing, payments, inventory, reports, and reviews.

---

## Cart Operations

### Add to Cart

```csharp
static void AddToCart(Customer customer)
{
    BrowseProducts();
    int id = ReadInt("\nEnter Product ID to add: ");
    var product = products.FirstOrDefault(p => p.Id == id);

    if (product == null)   { Console.WriteLine("Product not found."); return; }
    if (product.Stock == 0){ Console.WriteLine("Out of stock."); return; }

    int qty = ReadIntInRange("Quantity: ", 1, product.Stock);
    customer.Cart.AddItem(product, qty);
    Console.WriteLine($"✓ Added {qty}x {product.Name} to cart.");
}
```

### Update Cart

```csharp
static void UpdateCart(Customer customer)
{
    customer.Cart.Display();
    if (!customer.Cart.Items.Any()) return;

    int id = ReadInt("Product ID to update (0 to remove): ");
    var item = customer.Cart.Items.FirstOrDefault(i => i.Product.Id == id);
    if (item == null) { Console.WriteLine("Item not in cart."); return; }

    Console.WriteLine("1. Change quantity  2. Remove item");
    switch (Console.ReadLine()?.Trim())
    {
        case "1":
            int qty = ReadIntInRange("New quantity: ", 1, item.Product.Stock);
            item.Quantity = qty;
            Console.WriteLine("✓ Updated.");
            break;
        case "2":
            customer.Cart.RemoveItem(id);
            Console.WriteLine("✓ Removed.");
            break;
    }
}
```

---

## Checkout & Payment

```csharp
static void Checkout(Customer customer)
{
    if (!customer.Cart.Items.Any()) { Console.WriteLine("Cart is empty."); return; }

    customer.Cart.Display();
    Console.WriteLine($"\nTotal: R{customer.Cart.Total:F2}");
    Console.WriteLine($"Wallet balance: R{customer.WalletBalance:F2}");

    if (customer.WalletBalance < customer.Cart.Total)
    {
        Console.WriteLine("✗ Insufficient wallet balance. Add funds first.");
        return;
    }

    // Stock check
    foreach (var item in customer.Cart.Items)
    {
        if (item.Product.Stock < item.Quantity)
        {
            Console.WriteLine($"✗ Not enough stock for {item.Product.Name}.");
            return;
        }
    }

    Console.Write("Confirm order? (y/n): ");
    if (Console.ReadLine()?.Trim().ToLower() != "y") return;

    // Deduct stock
    foreach (var item in customer.Cart.Items)
        item.Product.Stock -= item.Quantity;

    // Build order
    var order = new Order
    {
        Id = nextOrderId++,
        CustomerId = customer.Id,
        Items = customer.Cart.Items.Select(i => new OrderItem
        {
            Product = i.Product,
            Quantity = i.Quantity,
            PriceAtPurchase = i.Product.Price
        }).ToList()
    };

    // Process payment
    customer.WalletBalance -= order.Total;
    payments.Add(new Payment
    {
        Id = payments.Count + 1,
        OrderId = order.Id,
        Amount = order.Total,
        Success = true
    });

    orders.Add(order);
    customer.Cart.Clear();

    Console.WriteLine($"✓ Order #{order.Id} placed. R{order.Total:F2} deducted. New balance: R{customer.WalletBalance:F2}");
}
```

---

## Wallet

```csharp
static void AddWalletFunds(Customer customer)
{
    decimal amount = ReadDecimal("Amount to add: R");
    if (amount <= 0) { Console.WriteLine("Amount must be positive."); return; }
    customer.WalletBalance += amount;
    Console.WriteLine($"✓ Wallet topped up. New balance: R{customer.WalletBalance:F2}");
}
```

---

## Product Reviews

```csharp
static void ReviewProduct(Customer customer)
{
    // Only allow reviews on delivered orders
    var deliveredProductIds = orders
        .Where(o => o.CustomerId == customer.Id && o.Status == OrderStatus.Delivered)
        .SelectMany(o => o.Items.Select(i => i.Product.Id))
        .Distinct()
        .ToList();

    if (!deliveredProductIds.Any())
    {
        Console.WriteLine("You have no delivered orders to review.");
        return;
    }

    var reviewableProducts = products.Where(p => deliveredProductIds.Contains(p.Id)).ToList();
    Console.WriteLine("Products you can review:");
    foreach (var p in reviewableProducts) p.Display();

    int id = ReadInt("Product ID to review: ");
    var product = reviewableProducts.FirstOrDefault(p => p.Id == id);
    if (product == null) { Console.WriteLine("Invalid selection."); return; }

    int rating = ReadIntInRange("Rating (1-5): ", 1, 5);
    Console.Write("Comment: ");
    string comment = Console.ReadLine();

    var review = new Review
    {
        Id = product.Reviews.Count + 1,
        ProductId = product.Id,
        CustomerId = customer.Id,
        CustomerName = customer.Name,
        Rating = rating,
        Comment = comment
    };

    product.AddReview(review);
    Console.WriteLine($"✓ Review submitted. New average: ★{product.AverageRating:F1}");
}
```

---

## Admin: Product CRUD

```csharp
static void AddProduct()
{
    Console.Write("Name: "); string name = ReadNonEmpty("Name");
    Console.Write("Category: "); string cat = ReadNonEmpty("Category");
    Console.Write("Description: "); string desc = Console.ReadLine();
    decimal price = ReadDecimal("Price: R");
    int stock = ReadInt("Initial stock: ");

    products.Add(new Product
    {
        Id = nextProductId++,
        Name = name, Category = cat, Description = desc,
        Price = price, Stock = stock
    });
    Console.WriteLine("✓ Product added.");
}

static void UpdateProduct()
{
    BrowseProducts();
    int id = ReadInt("Product ID to update: ");
    var p = products.FirstOrDefault(pr => pr.Id == id);
    if (p == null) { Console.WriteLine("Not found."); return; }

    Console.WriteLine($"Editing: {p.Name}");
    Console.Write($"New name ({p.Name}): ");
    string name = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(name)) p.Name = name;

    decimal price = ReadDecimal($"New price (current R{p.Price:F2}): R");
    if (price > 0) p.Price = price;

    Console.WriteLine("✓ Product updated.");
}

static void DeleteProduct()
{
    BrowseProducts();
    int id = ReadInt("Product ID to delete: ");
    int removed = products.RemoveAll(p => p.Id == id);
    Console.WriteLine(removed > 0 ? "✓ Deleted." : "Not found.");
}

static void RestockProduct()
{
    BrowseProducts();
    int id = ReadInt("Product ID to restock: ");
    var p = products.FirstOrDefault(pr => pr.Id == id);
    if (p == null) { Console.WriteLine("Not found."); return; }

    int qty = ReadInt("Units to add: ");
    p.Stock += qty;
    Console.WriteLine($"✓ {p.Name} restocked. New stock: {p.Stock}");
}
```

---

## Admin: Orders

```csharp
static void ViewAllOrders()
{
    if (!orders.Any()) { Console.WriteLine("No orders."); return; }
    foreach (var o in orders.OrderByDescending(o => o.PlacedAt))
        o.Display();
}

static void UpdateOrderStatus()
{
    ViewAllOrders();
    int id = ReadInt("Order ID: ");
    var order = orders.FirstOrDefault(o => o.Id == id);
    if (order == null) { Console.WriteLine("Not found."); return; }

    Console.WriteLine("Statuses: 1=Pending 2=Processing 3=Shipped 4=Delivered 5=Cancelled");
    int choice = ReadIntInRange("New status: ", 1, 5);
    order.Status = (OrderStatus)(choice - 1);
    Console.WriteLine($"✓ Order #{order.Id} → {order.Status}");
}
```

---

## Admin: Reports (LINQ showcase)

```csharp
static void ViewLowStock()
{
    var low = products.Where(p => p.Stock < 5).OrderBy(p => p.Stock).ToList();
    if (!low.Any()) { Console.WriteLine("All products adequately stocked."); return; }
    Console.WriteLine("=== Low Stock Alert ===");
    foreach (var p in low)
        Console.WriteLine($"  {p.Name} — Only {p.Stock} left");
}

static void GenerateSalesReport()
{
    Console.WriteLine("\n=== Sales Report ===");

    decimal totalRevenue = orders
        .Where(o => o.Status == OrderStatus.Delivered)
        .Sum(o => o.Total);

    int totalOrders = orders.Count;

    var topProducts = orders
        .SelectMany(o => o.Items)
        .GroupBy(i => i.Product.Name)
        .Select(g => new { Name = g.Key, Sold = g.Sum(i => i.Quantity) })
        .OrderByDescending(x => x.Sold)
        .Take(3)
        .ToList();

    var categoryRevenue = orders
        .SelectMany(o => o.Items)
        .GroupBy(i => i.Product.Category)
        .Select(g => new { Category = g.Key, Revenue = g.Sum(i => i.Subtotal) })
        .OrderByDescending(x => x.Revenue)
        .ToList();

    Console.WriteLine($"Total Revenue (Delivered): R{totalRevenue:F2}");
    Console.WriteLine($"Total Orders: {totalOrders}");

    Console.WriteLine("\nTop 3 Products:");
    foreach (var p in topProducts)
        Console.WriteLine($"  {p.Name} — {p.Sold} units sold");

    Console.WriteLine("\nRevenue by Category:");
    foreach (var c in categoryRevenue)
        Console.WriteLine($"  {c.Category}: R{c.Revenue:F2}");
}
```

---

## LINQ Patterns Quick Reference

| Task                 | LINQ approach                           |
| -------------------- | --------------------------------------- |
| Filter by condition  | `.Where(x => x.Prop == val)`            |
| Sort ascending       | `.OrderBy(x => x.Prop)`                 |
| Sort descending      | `.OrderByDescending(x => x.Prop)`       |
| First or null        | `.FirstOrDefault(x => x.Id == id)`      |
| Flatten nested lists | `.SelectMany(o => o.Items)`             |
| Group and aggregate  | `.GroupBy(x => x.Key).Select(g => ...)` |
| Sum a decimal field  | `.Sum(x => x.Amount)`                   |
| Count matches        | `.Count(x => x.Status == X)`            |
| Distinct values      | `.Select(x => x.Id).Distinct()`         |
| Top N                | `.Take(3)`                              |
