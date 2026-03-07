using ConsoleShoppingApp.Domain;

namespace ConsoleShoppingApp.Services;

public sealed class ReportService
{
    private readonly InMemoryStore _store;
    private readonly CatalogService _catalogService;

    public ReportService(InMemoryStore store, CatalogService catalogService)
    {
        _store = store;
        _catalogService = catalogService;
    }

    public List<Product> GetLowStockProducts(int threshold)
    {
        return _catalogService.GetAllProducts()
            .Where(p => p.StockQuantity <= threshold)
            .OrderBy(p => p.StockQuantity)
            .ToList();
    }

    public SalesReport GenerateSalesReport()
    {
        var paidOrders = _store.Orders
            .Where(o => o.Status != OrderStatus.Cancelled)
            .ToList();

        var totalOrders = paidOrders.Count;
        var totalRevenue = paidOrders.Sum(o => o.TotalAmount);
        var averageOrderValue = totalOrders == 0 ? 0 : totalRevenue / totalOrders;

        var productSales = paidOrders
            .SelectMany(order => order.Items)
            .GroupBy(item => item.ProductId)
            .Select(group => new ProductSales(
                group.Key,
                group.First().ProductName,
                group.Sum(item => item.Quantity),
                group.Sum(item => item.Quantity * item.UnitPrice)))
            .OrderByDescending(x => x.QuantitySold)
            .ThenByDescending(x => x.Revenue)
            .ToList();

        var topProductName = productSales.FirstOrDefault()?.ProductName ?? "N/A";
        var topProducts = productSales.Take(5).ToList();

        return new SalesReport(totalOrders, totalRevenue, averageOrderValue, topProductName, topProducts);
    }
}

public sealed record SalesReport(
    int TotalOrders,
    decimal TotalRevenue,
    decimal AverageOrderValue,
    string TopProductName,
    List<ProductSales> TopProducts);

public sealed record ProductSales(
    Guid ProductId,
    string ProductName,
    int QuantitySold,
    decimal Revenue);
