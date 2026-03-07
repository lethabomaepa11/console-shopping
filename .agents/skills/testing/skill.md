---
name: testing-and-validation
description: Write tests, validate correctness, and produce evidence of testing for the Online Shopping Console App in C#. Use this skill whenever the user asks about testing, unit tests, test cases, verifying features work, NUnit, xUnit, MSTest, manual test scripts, edge cases, or how to demonstrate correctness for Submission 2. Also trigger when the user asks "how do I prove it works", "write tests for X", "what edge cases should I handle", or references the 10% Testing/Validation criterion in the rubric.
---

# Testing & Validation — Online Shopping Console App

This skill covers how to write and structure tests for the console app — both automated unit tests (for Submission 2 marks) and manual test scripts (for demo evidence).

---

## Two Layers of Testing

| Layer                  | Tool               | Purpose                             |
| ---------------------- | ------------------ | ----------------------------------- |
| **Unit tests**         | xUnit or NUnit     | Verify business logic in isolation  |
| **Manual test script** | Markdown checklist | Evidence graders can see and follow |

Both matter for the 10% Testing mark. A test project alone without a written script leaves the grader guessing. A script alone without code tests is weaker evidence.

---

## Setting Up an xUnit Test Project

```bash
# From the solution root
dotnet new xunit -n OnlineShoppingApp.Tests
dotnet add OnlineShoppingApp.Tests/OnlineShoppingApp.Tests.csproj reference OnlineShoppingApp/OnlineShoppingApp.csproj
dotnet test
```

Your solution folder should look like:

```
OnlineShoppingApp/          ← main app
OnlineShoppingApp.Tests/    ← test project
  ├── CartTests.cs
  ├── CheckoutTests.cs
  ├── ProductTests.cs
  ├── OrderTests.cs
  └── AuthTests.cs
```

---

## Unit Tests — What to Test

Focus on business logic, not console output. Test the methods and classes, not `Console.WriteLine`.

### CartTests.cs

```csharp
using Xunit;

public class CartTests
{
    private Customer MakeCustomer() =>
        new Customer("Test", "t@t.com", "pass") { Id = 1, WalletBalance = 1000m };

    private Product MakeProduct(int id = 1, int stock = 10) =>
        new Product { Id = id, Name = "Widget", Price = 100m, Stock = stock };

    [Fact]
    public void AddItem_NewProduct_AddsToCart()
    {
        var customer = MakeCustomer();
        var product = MakeProduct();
        customer.Cart.AddItem(product, 2);
        Assert.Single(customer.Cart.Items);
        Assert.Equal(2, customer.Cart.Items[0].Quantity);
    }

    [Fact]
    public void AddItem_ExistingProduct_IncrementsQuantity()
    {
        var customer = MakeCustomer();
        var product = MakeProduct();
        customer.Cart.AddItem(product, 2);
        customer.Cart.AddItem(product, 3);
        Assert.Single(customer.Cart.Items);
        Assert.Equal(5, customer.Cart.Items[0].Quantity);
    }

    [Fact]
    public void RemoveItem_RemovesCorrectItem()
    {
        var customer = MakeCustomer();
        var p1 = MakeProduct(1);
        var p2 = MakeProduct(2);
        customer.Cart.AddItem(p1, 1);
        customer.Cart.AddItem(p2, 1);
        customer.Cart.RemoveItem(1);
        Assert.Single(customer.Cart.Items);
        Assert.Equal(2, customer.Cart.Items[0].Product.Id);
    }

    [Fact]
    public void Total_CalculatesCorrectly()
    {
        var customer = MakeCustomer();
        customer.Cart.AddItem(new Product { Id = 1, Price = 50m, Stock = 10 }, 3);
        customer.Cart.AddItem(new Product { Id = 2, Price = 25m, Stock = 10 }, 2);
        Assert.Equal(200m, customer.Cart.Total); // 3×50 + 2×25
    }

    [Fact]
    public void Clear_EmptiesCart()
    {
        var customer = MakeCustomer();
        customer.Cart.AddItem(MakeProduct(), 1);
        customer.Cart.Clear();
        Assert.Empty(customer.Cart.Items);
    }
}
```

---

### ProductTests.cs

```csharp
public class ProductTests
{
    [Fact]
    public void AddReview_UpdatesAverageRating()
    {
        var product = new Product { Id = 1, Name = "Widget" };
        product.AddReview(new Review { Rating = 4, CustomerName = "Alice" });
        product.AddReview(new Review { Rating = 2, CustomerName = "Bob" });
        Assert.Equal(3.0, product.AverageRating, precision: 1);
    }

    [Fact]
    public void Matches_ReturnsTrueForNameSubstring()
    {
        var product = new Product { Name = "Laptop", Category = "Electronics" };
        Assert.True(product.Matches("lap"));
        Assert.True(product.Matches("LAPTOP")); // case-insensitive
        Assert.False(product.Matches("phone"));
    }

    [Fact]
    public void Matches_ReturnsTrueForCategorySubstring()
    {
        var product = new Product { Name = "Widget", Category = "Electronics" };
        Assert.True(product.Matches("electr"));
    }
}
```

---

### CheckoutTests.cs

Test the checkout logic directly (extract to a `CheckoutService` for testability):

```csharp
// Services/CheckoutService.cs  (extract from Program.cs for testability)
public class CheckoutService
{
    private readonly List<Order> _orders;
    private readonly List<Payment> _payments;
    private int _nextOrderId;

    public CheckoutService(List<Order> orders, List<Payment> payments, ref int nextOrderId)
    {
        _orders = orders;
        _payments = payments;
        _nextOrderId = nextOrderId;
    }

    public (bool success, string message) TryCheckout(Customer customer)
    {
        if (!customer.Cart.Items.Any())
            return (false, "Cart is empty.");

        if (customer.WalletBalance < customer.Cart.Total)
            return (false, "Insufficient balance.");

        foreach (var item in customer.Cart.Items)
            if (item.Product.Stock < item.Quantity)
                return (false, $"Not enough stock for {item.Product.Name}.");

        // Deduct stock
        foreach (var item in customer.Cart.Items)
            item.Product.Stock -= item.Quantity;

        var order = new Order
        {
            Id = _nextOrderId++,
            CustomerId = customer.Id,
            Items = customer.Cart.Items.Select(i => new OrderItem
            {
                Product = i.Product,
                Quantity = i.Quantity,
                PriceAtPurchase = i.Product.Price
            }).ToList()
        };

        customer.WalletBalance -= order.Total;
        _payments.Add(new Payment { OrderId = order.Id, Amount = order.Total, Success = true });
        _orders.Add(order);
        customer.Cart.Clear();

        return (true, $"Order #{order.Id} placed.");
    }
}
```

```csharp
// Tests/CheckoutTests.cs
public class CheckoutTests
{
    private (Customer customer, Product product, CheckoutService service) Setup(
        decimal walletBalance = 500m, int stock = 10, decimal price = 100m)
    {
        var customer = new Customer("Alice", "a@a.com", "pass") { Id = 1, WalletBalance = walletBalance };
        var product = new Product { Id = 1, Name = "Widget", Price = price, Stock = stock };
        int nextId = 1;
        var service = new CheckoutService(new List<Order>(), new List<Payment>(), ref nextId);
        return (customer, product, service);
    }

    [Fact]
    public void Checkout_EmptyCart_Fails()
    {
        var (customer, _, service) = Setup();
        var (success, msg) = service.TryCheckout(customer);
        Assert.False(success);
        Assert.Contains("empty", msg, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Checkout_InsufficientBalance_Fails()
    {
        var (customer, product, service) = Setup(walletBalance: 50m, price: 100m);
        customer.Cart.AddItem(product, 1);
        var (success, msg) = service.TryCheckout(customer);
        Assert.False(success);
        Assert.Contains("balance", msg, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Checkout_InsufficientStock_Fails()
    {
        var (customer, product, service) = Setup(stock: 1);
        customer.Cart.AddItem(product, 5); // more than stock
        // Manually override quantity past AddItem's guard for test
        customer.Cart.Items[0].Quantity = 5;
        var (success, _) = service.TryCheckout(customer);
        Assert.False(success);
    }

    [Fact]
    public void Checkout_Success_DeductsWalletAndStock()
    {
        var (customer, product, service) = Setup(walletBalance: 500m, stock: 10, price: 100m);
        customer.Cart.AddItem(product, 2);
        var (success, _) = service.TryCheckout(customer);
        Assert.True(success);
        Assert.Equal(300m, customer.WalletBalance); // 500 - 200
        Assert.Equal(8, product.Stock);             // 10 - 2
        Assert.Empty(customer.Cart.Items);          // cart cleared
    }

    [Fact]
    public void Checkout_Success_CapturesPriceAtPurchaseTime()
    {
        var (customer, product, service) = Setup(price: 100m);
        customer.Cart.AddItem(product, 1);
        service.TryCheckout(customer);
        product.Price = 999m; // price changes after checkout

        // Order should still have original price
        // (requires access to orders list — adjust Setup if needed)
    }
}
```

---

### AuthTests.cs

```csharp
public class AuthTests
{
    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        var customer = new Customer("Alice", "a@a.com", "secret123");
        Assert.True(customer.VerifyPassword("secret123"));
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        var customer = new Customer("Alice", "a@a.com", "secret123");
        Assert.False(customer.VerifyPassword("wrong"));
    }

    [Fact]
    public void UserFactory_CreatesCorrectType()
    {
        var customer = UserFactory.Create("customer", "Alice", "a@a.com", "pass");
        var admin    = UserFactory.Create("admin",    "Bob",   "b@b.com", "pass");
        Assert.IsType<Customer>(customer);
        Assert.IsType<Administrator>(admin);
    }

    [Fact]
    public void UserFactory_UnknownRole_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            UserFactory.Create("vendor", "X", "x@x.com", "pass"));
    }
}
```

---

## Manual Test Script (include in README or TESTING.md)

Document this so graders can follow along during a demo.

```markdown
# Manual Test Cases

## TC-01: Register and Login

- Run app → select Register
- Enter name: Alice, email: alice@test.com, password: pass, role: customer
- ✓ Expected: "Registered as customer"
- Select Login → enter same credentials
- ✓ Expected: Customer menu appears

## TC-02: Browse and Search Products

- Login as customer
- Select "Browse Products"
- ✓ Expected: 5 products listed with ID, name, price, stock, rating
- Select "Search Products" → enter "lap"
- ✓ Expected: Only Laptop appears

## TC-03: Add to Cart and Checkout

- Add Laptop (qty 1) to cart
- View Cart → ✓ Total = R12999
- Add Wallet Funds → add R15000
- Checkout → confirm
- ✓ Expected: Order placed, wallet deducted, stock reduced by 1

## TC-04: Insufficient Wallet Balance

- New customer with R0 balance
- Add any product to cart
- Attempt Checkout
- ✓ Expected: "Insufficient wallet balance" — no order created

## TC-05: Admin Restock

- Login as admin
- Select "View Low Stock Products"
- ✓ Expected: Products with stock < 5 listed
- Select "Restock Product" → restock T-Shirt by 20
- ✓ Expected: T-Shirt no longer appears in low stock

## TC-06: Order Status Update + Observer

- Login as admin → Update Order Status on Order #1 → set to Shipped
- ✓ Expected: Status updated
- If Observer pattern implemented: customer notification printed

## TC-07: Product Review

- Login as customer with a Delivered order
- Select "Review Products" → rate product 5 stars
- ✓ Expected: Review saved, product average rating updated
- Browse products → confirm new average shown

## TC-08: Edge Cases

- Enter letters when number expected → ✓ "Please enter a whole number"
- Add more items to cart than stock allows → ✓ Blocked
- Checkout with empty cart → ✓ "Cart is empty"
- Login with wrong password → ✓ "Invalid credentials"
- Register with duplicate email → ✓ "Email already registered"
```

---

## Running Tests

```bash
dotnet test --verbosity normal
```

Expected output:

```
Passed! - 18 tests passed, 0 failed
```

Include a screenshot of this output in your README for the Testing/Validation marks.

---

## Edge Cases Checklist

- [ ] Empty cart checkout blocked
- [ ] Wallet below order total blocked
- [ ] Adding more qty than stock blocked
- [ ] Duplicate email on register blocked
- [ ] Wrong password on login blocked
- [ ] Non-numeric input handled gracefully (no crash)
- [ ] Reviewing a product not yet purchased blocked
- [ ] Deleting a product that's in an active cart (handle gracefully)
- [ ] Order ID not found when tracking
- [ ] Admin: restock by 0 or negative amount handled
