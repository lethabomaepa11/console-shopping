---
name: project-architecture
description: Set up the full project structure, file layout, and overall scaffolding for the Online Shopping Console App in C#. Use this skill whenever the user asks to start the project from scratch, set up the solution, create the folder structure, wire everything together in Program.cs, or asks "where do I start". Also trigger when the user asks how all the pieces connect, wants a high-level overview of the project, needs a complete working starting point, or asks about the overall architecture of the system. This is the entry point skill — read this first before diving into domain models or patterns.
---

# Project Architecture — Online Shopping Console App

This skill is the starting point. It gives you the full file layout, how to create the project, and how all the pieces connect — for both Submission 1 and Submission 2.

---

## Quick Start

```bash
# Create a new .NET console app
dotnet new console -n OnlineShoppingApp
cd OnlineShoppingApp
dotnet run
```

No external packages needed for Submission 1. Standard library only.

---

## Submission 1 File Layout

Keep it simple. Most logic can live in `Program.cs` for Sub 1.

```
OnlineShoppingApp/
├── OnlineShoppingApp.csproj
├── Program.cs                ← Main entry point, menus, all logic
├── Models/
│   ├── User.cs
│   ├── Customer.cs
│   ├── Administrator.cs
│   ├── Product.cs
│   ├── Cart.cs
│   ├── CartItem.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   ├── Payment.cs
│   └── Review.cs
└── Interfaces/
    ├── ISearchable.cs
    ├── IReportable.cs
    └── IPayable.cs
```

**Build order:** Models → Interfaces → Program.cs logic → Menus

---

## Submission 2 File Layout

Introduce layers. Move logic out of `Program.cs`.

```
OnlineShoppingApp/
├── OnlineShoppingApp.csproj
├── Program.cs                ← Entry only, wires up DI/services
├── Models/                   ← Unchanged from Sub 1
├── Interfaces/               ← Unchanged + add IPaymentStrategy
├── Patterns/
│   ├── DataStore.cs          (Singleton)
│   ├── UserFactory.cs        (Factory)
│   ├── IOrderObserver.cs
│   └── OrderNotificationManager.cs  (Observer)
├── Repositories/
│   ├── IRepository.cs
│   ├── ProductRepository.cs
│   └── OrderRepository.cs
├── Services/
│   ├── AuthService.cs
│   ├── CheckoutService.cs    (includes Strategy)
│   └── ReportService.cs
└── UI/
    ├── CustomerMenu.cs
    └── AdminMenu.cs
```

---

## How the Pieces Connect

```
Program.cs
  │
  ├── SeedData() → DataStore.Instance (Singleton)
  │
  ├── ShowMainMenu()
  │     ├── Register() → UserFactory.Create() → DataStore.Users
  │     └── Login()    → AuthService.Login()
  │                          ├── Customer → CustomerMenu
  │                          └── Admin    → AdminMenu
  │
  ├── CustomerMenu
  │     ├── Cart ops      → customer.Cart (domain logic)
  │     ├── Checkout      → CheckoutService (Strategy pattern)
  │     ├── Order history → OrderRepository.GetByCustomer()
  │     └── Reviews       → ProductRepository.AddReview()
  │
  └── AdminMenu
        ├── Product CRUD  → ProductRepository
        ├── Order mgmt    → OrderRepository + Observer notifications
        └── Reports       → ReportService (LINQ aggregations)
```

---

## Program.cs Skeleton (Submission 1)

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    // --- Data Collections ---
    static List<User> users = new();
    static List<Product> products = new();
    static List<Order> orders = new();
    static List<Payment> payments = new();
    static int nextUserId = 1, nextProductId = 1, nextOrderId = 1;

    // --- Entry Point ---
    static void Main(string[] args)
    {
        SeedData();
        Console.WriteLine("╔══════════════════════════╗");
        Console.WriteLine("║   Online Shopping App    ║");
        Console.WriteLine("╚══════════════════════════╝");
        ShowMainMenu();
    }

    // ===== MENUS =====
    static void ShowMainMenu() { /* see console-menus-ui skill */ }
    static void ShowCustomerMenu(Customer c) { /* see console-menus-ui skill */ }
    static void ShowAdminMenu(Administrator a) { /* see console-menus-ui skill */ }

    // ===== AUTH =====
    static void Register() { /* see console-menus-ui skill */ }
    static void Login() { /* see console-menus-ui skill */ }

    // ===== CUSTOMER FEATURES =====
    static void BrowseProducts() { /* see linq-and-data skill */ }
    static void SearchProducts() { /* see linq-and-data skill */ }
    static void AddToCart(Customer c) { /* see linq-and-data skill */ }
    static void UpdateCart(Customer c) { /* see linq-and-data skill */ }
    static void Checkout(Customer c) { /* see linq-and-data skill */ }
    static void AddWalletFunds(Customer c) { /* see linq-and-data skill */ }
    static void ViewOrderHistory(Customer c) { /* see linq-and-data skill */ }
    static void TrackOrders(Customer c) { /* see linq-and-data skill */ }
    static void ReviewProduct(Customer c) { /* see linq-and-data skill */ }

    // ===== ADMIN FEATURES =====
    static void AddProduct() { /* see linq-and-data skill */ }
    static void UpdateProduct() { /* see linq-and-data skill */ }
    static void DeleteProduct() { /* see linq-and-data skill */ }
    static void RestockProduct() { /* see linq-and-data skill */ }
    static void ViewAllOrders() { /* see linq-and-data skill */ }
    static void UpdateOrderStatus() { /* see linq-and-data skill */ }
    static void ViewLowStock() { /* see linq-and-data skill */ }
    static void GenerateSalesReport() { /* see linq-and-data skill */ }

    // ===== INPUT HELPERS =====
    static string ReadNonEmpty(string f) { /* see console-menus-ui skill */ }
    static int ReadInt(string p) { /* see console-menus-ui skill */ }
    static decimal ReadDecimal(string p) { /* see console-menus-ui skill */ }
    static int ReadIntInRange(string p, int min, int max) { /* see console-menus-ui skill */ }

    // ===== SEED DATA =====
    static void SeedData() { /* see console-menus-ui skill */ }
}
```

---

## .csproj Settings

Ensure your `.csproj` targets .NET 6+ to use modern C# features (`??=`, switch expressions, `new()` initializers):

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

---

## Which Skill to Use Next

| Task                                  | Skill                    |
| ------------------------------------- | ------------------------ |
| Building User, Product, Order classes | `oop-domain-models`      |
| Building menus and console input      | `console-menus-ui`       |
| Checkout, reports, LINQ queries       | `linq-and-data`          |
| Submission 2 patterns and refactoring | `design-patterns-sub2`   |
| Unit tests, edge cases, test evidence | `testing-and-validation` |

---

## Common Mistakes to Avoid

- Don't put `using` statements inside namespace blocks (unless intentional)
- `Cart` should be initialised inside `Customer`'s constructor, not separately
- `OrderItem.PriceAtPurchase` must be captured at checkout time — not read from `Product.Price` later
- Always decrement `Product.Stock` at checkout, not when adding to cart
- `OrderStatus` must be an `enum`, not a `string`
- Seed data must run before `ShowMainMenu()` so menus are always demonstrable
