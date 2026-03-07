---
name: console-menus-ui
description: Build and fix the console menu system, user interaction loops, input validation, and all user-facing console output for the Online Shopping Console App. Use this skill whenever the user asks to implement or improve any menu (Main, Customer, Admin), handle console input, display products/cart/orders, validate user input, or wire up menu options to logic. Trigger on phrases like "build the menu", "add the customer menu", "handle user input", "display products", "console interface", or any reference to a numbered menu option from the spec.
---

# Console Menus & UI — Online Shopping Console App

This skill covers the full console interaction layer: menus, input loops, display helpers, and input validation.

---

## Program Entry Point

```csharp
// Program.cs
class Program
{
    static List<User> users = new();
    static List<Product> products = new();
    static List<Order> orders = new();
    static List<Payment> payments = new();
    static int nextUserId = 1, nextProductId = 1, nextOrderId = 1;

    static void Main(string[] args)
    {
        SeedData();
        Console.WriteLine("=== Welcome to ShopSim ===");
        ShowMainMenu();
    }
}
```

---

## Main Menu

```csharp
static void ShowMainMenu()
{
    while (true)
    {
        Console.WriteLine("\n=== Main Menu ===");
        Console.WriteLine("1. Register");
        Console.WriteLine("2. Login");
        Console.WriteLine("3. Exit");
        Console.Write("Select: ");

        switch (Console.ReadLine()?.Trim())
        {
            case "1": Register(); break;
            case "2": Login(); break;
            case "3": Console.WriteLine("Goodbye!"); return;
            default:  Console.WriteLine("Invalid option."); break;
        }
    }
}
```

---

## Registration & Login

```csharp
static void Register()
{
    Console.Write("Name: ");
    string name = ReadNonEmpty("Name");
    Console.Write("Email: ");
    string email = ReadNonEmpty("Email");
    Console.Write("Password: ");
    string password = ReadNonEmpty("Password");
    Console.Write("Role (customer/admin): ");
    string role = Console.ReadLine()?.Trim().ToLower();

    if (users.Any(u => u.Email == email))
    { Console.WriteLine("Email already registered."); return; }

    User user = role == "admin"
        ? new Administrator(name, email, password) { Id = nextUserId++ }
        : new Customer(name, email, password) { Id = nextUserId++ };

    users.Add(user);
    Console.WriteLine($"✓ Registered as {role}.");
}

static void Login()
{
    Console.Write("Email: ");
    string email = Console.ReadLine()?.Trim();
    Console.Write("Password: ");
    string password = Console.ReadLine();

    var user = users.FirstOrDefault(u => u.Email == email && u.VerifyPassword(password));
    if (user == null) { Console.WriteLine("Invalid credentials."); return; }

    user.DisplayDashboard();
    if (user is Customer c) ShowCustomerMenu(c);
    else if (user is Administrator a) ShowAdminMenu(a);
}
```

---

## Customer Menu

```csharp
static void ShowCustomerMenu(Customer customer)
{
    bool running = true;
    while (running)
    {
        Console.WriteLine("\n=== Customer Menu ===");
        Console.WriteLine("1.  Browse Products");
        Console.WriteLine("2.  Search Products");
        Console.WriteLine("3.  Add Product to Cart");
        Console.WriteLine("4.  View Cart");
        Console.WriteLine("5.  Update Cart");
        Console.WriteLine("6.  Checkout");
        Console.WriteLine("7.  View Wallet Balance");
        Console.WriteLine("8.  Add Wallet Funds");
        Console.WriteLine("9.  View Order History");
        Console.WriteLine("10. Track Orders");
        Console.WriteLine("11. Review Products");
        Console.WriteLine("12. Logout");
        Console.Write("Select: ");

        switch (Console.ReadLine()?.Trim())
        {
            case "1":  BrowseProducts(); break;
            case "2":  SearchProducts(); break;
            case "3":  AddToCart(customer); break;
            case "4":  customer.Cart.Display(); break;
            case "5":  UpdateCart(customer); break;
            case "6":  Checkout(customer); break;
            case "7":  Console.WriteLine($"Wallet: R{customer.WalletBalance:F2}"); break;
            case "8":  AddWalletFunds(customer); break;
            case "9":  ViewOrderHistory(customer); break;
            case "10": TrackOrders(customer); break;
            case "11": ReviewProduct(customer); break;
            case "12": running = false; break;
            default:   Console.WriteLine("Invalid option."); break;
        }
    }
}
```

---

## Administrator Menu

```csharp
static void ShowAdminMenu(Administrator admin)
{
    bool running = true;
    while (running)
    {
        Console.WriteLine("\n=== Administrator Menu ===");
        Console.WriteLine("1.  Add Product");
        Console.WriteLine("2.  Update Product");
        Console.WriteLine("3.  Delete Product");
        Console.WriteLine("4.  Restock Product");
        Console.WriteLine("5.  View Products");
        Console.WriteLine("6.  View Orders");
        Console.WriteLine("7.  Update Order Status");
        Console.WriteLine("8.  View Low Stock Products");
        Console.WriteLine("9.  Generate Sales Reports");
        Console.WriteLine("10. Logout");
        Console.Write("Select: ");

        switch (Console.ReadLine()?.Trim())
        {
            case "1":  AddProduct(); break;
            case "2":  UpdateProduct(); break;
            case "3":  DeleteProduct(); break;
            case "4":  RestockProduct(); break;
            case "5":  BrowseProducts(); break;
            case "6":  ViewAllOrders(); break;
            case "7":  UpdateOrderStatus(); break;
            case "8":  ViewLowStock(); break;
            case "9":  GenerateSalesReport(); break;
            case "10": running = false; break;
            default:   Console.WriteLine("Invalid option."); break;
        }
    }
}
```

---

## Input Helpers

```csharp
// Read non-empty string
static string ReadNonEmpty(string fieldName)
{
    string value;
    do {
        value = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(value))
            Console.Write($"{fieldName} cannot be empty. Try again: ");
    } while (string.IsNullOrWhiteSpace(value));
    return value;
}

// Read a valid integer
static int ReadInt(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        if (int.TryParse(Console.ReadLine(), out int result)) return result;
        Console.WriteLine("Please enter a whole number.");
    }
}

// Read a valid decimal
static decimal ReadDecimal(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        if (decimal.TryParse(Console.ReadLine(), out decimal result)) return result;
        Console.WriteLine("Please enter a valid amount.");
    }
}

// Read a valid int within a range
static int ReadIntInRange(string prompt, int min, int max)
{
    while (true)
    {
        int val = ReadInt(prompt);
        if (val >= min && val <= max) return val;
        Console.WriteLine($"Enter a number between {min} and {max}.");
    }
}
```

---

## Display Helpers

```csharp
static void BrowseProducts()
{
    if (!products.Any()) { Console.WriteLine("No products available."); return; }
    Console.WriteLine("\n=== Products ===");
    foreach (var p in products) p.Display();
}

static void SearchProducts()
{
    Console.Write("Search query: ");
    string query = Console.ReadLine()?.Trim();
    var results = products.Where(p => p.Matches(query)).ToList();

    if (!results.Any()) { Console.WriteLine("No matches found."); return; }
    Console.WriteLine($"\n--- Results for '{query}' ---");
    foreach (var p in results) p.Display();
}

static void ViewOrderHistory(Customer customer)
{
    var history = orders.Where(o => o.CustomerId == customer.Id).ToList();
    if (!history.Any()) { Console.WriteLine("No orders yet."); return; }
    foreach (var o in history) o.Display();
}

static void TrackOrders(Customer customer)
{
    int id = ReadInt("Enter Order ID: ");
    var order = orders.FirstOrDefault(o => o.Id == id && o.CustomerId == customer.Id);
    if (order == null) { Console.WriteLine("Order not found."); return; }
    Console.WriteLine($"Order #{order.Id} Status: {order.Status}");
}
```

---

## Key UX Rules

- Always confirm success: `Console.WriteLine("✓ Done.")`
- Always explain failure: `Console.WriteLine("✗ Not enough stock.")`
- Use `\n` before section headers to space output
- Never crash on bad input — always use `TryParse` or the `ReadInt`/`ReadDecimal` helpers
- Print menu header at the top of every loop iteration so the user always sees their options

---

## Seed Data (put in Program.cs)

```csharp
static void SeedData()
{
    products.AddRange(new[] {
        new Product { Id=nextProductId++, Name="Laptop",    Category="Electronics", Price=12999m, Stock=10 },
        new Product { Id=nextProductId++, Name="Headphones",Category="Electronics", Price=799m,   Stock=25 },
        new Product { Id=nextProductId++, Name="T-Shirt",   Category="Clothing",    Price=199m,   Stock=3  },
        new Product { Id=nextProductId++, Name="Coffee",    Category="Food",        Price=89m,    Stock=50 },
        new Product { Id=nextProductId++, Name="Notebook",  Category="Stationery",  Price=49m,    Stock=4  },
    });

    users.Add(new Customer("Alice", "alice@test.com", "pass123") { Id=nextUserId++, WalletBalance=5000m });
    users.Add(new Administrator("Admin", "admin@test.com", "admin123") { Id=nextUserId++ });
}
```
