using ConsoleShoppingApp.Domain.Models;
using ConsoleShoppingApp.Domain;

namespace ConsoleShoppingApp.Services;

public sealed class SimulationService
{
    private readonly InMemoryStore _store;

    public SimulationService(InMemoryStore store)
    {
        _store = store;
    }

    public SimulationReport RunDigitalTwin(int syntheticCustomers, int maxOrdersPerCustomer)
    {
        var availableProducts = _store.Products
            .Where(p => !p.IsDeleted && p.StockQuantity > 0)
            .ToList();

        if (!availableProducts.Any())
        {
            return new SimulationReport(0, 0, 0m, 0m, new List<SimulatedTopProduct>());
        }

        var random = new Random();
        var totalOrders = 0;
        var totalItems = 0;
        decimal totalRevenue = 0m;
        var productUnits = new Dictionary<Guid, int>();

        for (var c = 0; c < syntheticCustomers; c++)
        {
            var ordersForCustomer = random.Next(0, Math.Max(1, maxOrdersPerCustomer + 1));
            for (var o = 0; o < ordersForCustomer; o++)
            {
                var lines = random.Next(1, Math.Min(4, availableProducts.Count + 1));
                var selectedProducts = availableProducts
                    .OrderBy(_ => random.Next())
                    .Take(lines)
                    .ToList();

                decimal orderTotal = 0m;
                foreach (var product in selectedProducts)
                {
                    var quantity = random.Next(1, 4);
                    totalItems += quantity;
                    orderTotal += quantity * product.Price;

                    if (!productUnits.ContainsKey(product.Id))
                    {
                        productUnits[product.Id] = 0;
                    }

                    productUnits[product.Id] += quantity;
                }

                totalOrders++;
                totalRevenue += orderTotal;
            }
        }

        var topProducts = productUnits
            .Select(kvp =>
            {
                var product = availableProducts.First(p => p.Id == kvp.Key);
                return new SimulatedTopProduct(product.Name, kvp.Value);
            })
            .OrderByDescending(x => x.UnitsSold)
            .Take(5)
            .ToList();

        var averageOrderValue = totalOrders == 0 ? 0m : totalRevenue / totalOrders;
        return new SimulationReport(syntheticCustomers, totalOrders, totalRevenue, averageOrderValue, topProducts);
    }
}

public sealed record SimulationReport(
    int SyntheticCustomers,
    int SimulatedOrders,
    decimal SimulatedRevenue,
    decimal AverageOrderValue,
    List<SimulatedTopProduct> TopProducts);

public sealed record SimulatedTopProduct(string ProductName, int UnitsSold);
