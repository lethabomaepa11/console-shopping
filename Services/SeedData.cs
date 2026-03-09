using ConsoleShoppingApp.Domain.Models;
using ConsoleShoppingApp.Domain;

namespace ConsoleShoppingApp.Services;

public static class SeedData
{
    public static void Initialize(InMemoryStore store, AuthService authService, CatalogService catalogService)
    {
        if (store.Administrators.Any() || store.Customers.Any() || store.Products.Any())
        {
            return;
        }

        authService.Register("admin", "admin123", "System Admin", UserRole.Administrator);
        var customer = (Customer)authService.Register("customer", "cust123", "Default Customer", UserRole.Customer);
        customer.WalletBalance = 5000m;

        catalogService.AddProduct("Laptop", "15-inch business laptop", "Electronics", 1299.99m, 12);
        catalogService.AddProduct("Headphones", "Noise-cancelling headphones", "Electronics", 199.50m, 40);
        catalogService.AddProduct("Desk Chair", "Ergonomic office chair", "Furniture", 320m, 8);
        catalogService.AddProduct("Water Bottle", "1L stainless steel bottle", "Lifestyle", 25m, 100);
    }
}
