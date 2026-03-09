using ConsoleShoppingApp.Domain.Models;
using ConsoleShoppingApp.Domain;

namespace ConsoleShoppingApp.Services;

public sealed class RecommendationService
{
    private readonly InMemoryStore _store;
    private readonly CatalogService _catalogService;
    private readonly OrderService _orderService;

    public RecommendationService(InMemoryStore store, CatalogService catalogService, OrderService orderService)
    {
        _store = store;
        _catalogService = catalogService;
        _orderService = orderService;
    }

    public List<ProductRecommendation> GetRecommendations(Guid customerId, int topN = 5)
    {
        var available = _catalogService.GetAvailableProducts();
        if (!available.Any())
        {
            return new List<ProductRecommendation>();
        }

        var purchased = _orderService.GetCustomerOrders(customerId)
            .SelectMany(o => o.Items)
            .Select(i => i.ProductId)
            .ToHashSet();

        var preferredCategories = _store.Reviews
            .Where(r => r.CustomerId == customerId && r.Rating >= 4)
            .Select(r => _store.Products.FirstOrDefault(p => p.Id == r.ProductId)?.Category)
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Select(category => category!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var productId in purchased)
        {
            var product = _store.Products.FirstOrDefault(p => p.Id == productId);
            if (product is not null)
            {
                preferredCategories.Add(product.Category);
            }
        }

        var peerCustomerIds = _store.Orders
            .Where(o => o.CustomerId != customerId && o.Items.Any(i => purchased.Contains(i.ProductId)))
            .Select(o => o.CustomerId)
            .Distinct()
            .ToHashSet();

        var peerProductScores = _store.Orders
            .Where(o => peerCustomerIds.Contains(o.CustomerId))
            .SelectMany(o => o.Items)
            .Where(i => !purchased.Contains(i.ProductId))
            .GroupBy(i => i.ProductId)
            .ToDictionary(group => group.Key, group => group.Sum(i => i.Quantity));

        var productRatings = _store.Reviews
            .GroupBy(r => r.ProductId)
            .ToDictionary(group => group.Key, group => group.Average(r => r.Rating));

        var productSales = _store.Orders
            .Where(o => o.Status != OrderStatus.Cancelled)
            .SelectMany(o => o.Items)
            .GroupBy(i => i.ProductId)
            .ToDictionary(group => group.Key, group => group.Sum(i => i.Quantity));

        var scored = new List<ProductRecommendation>();

        foreach (var product in available.Where(p => !purchased.Contains(p.Id)))
        {
            var score = 0.0;
            var reasons = new List<string>();

            if (preferredCategories.Contains(product.Category))
            {
                score += 3.0;
                reasons.Add("matches your preferred category");
            }

            if (peerProductScores.TryGetValue(product.Id, out var peerScore) && peerScore > 0)
            {
                score += Math.Min(peerScore, 5);
                reasons.Add("popular with similar customers");
            }

            if (productRatings.TryGetValue(product.Id, out var avgRating))
            {
                score += avgRating / 2.0;
                if (avgRating >= 4.0)
                {
                    reasons.Add("highly rated");
                }
            }

            if (productSales.TryGetValue(product.Id, out var salesVolume))
            {
                score += Math.Min(salesVolume / 10.0, 2.0);
                if (salesVolume > 0)
                {
                    reasons.Add("strong recent demand");
                }
            }

            if (score <= 0)
            {
                score = 0.5;
                reasons.Add("popular fallback pick");
            }

            scored.Add(new ProductRecommendation(product, Math.Round(score, 2), reasons.Distinct().ToList()));
        }

        return scored
            .OrderByDescending(r => r.Score)
            .ThenBy(r => r.Product.Name)
            .Take(topN)
            .ToList();
    }
}

public sealed record ProductRecommendation(Product Product, double Score, List<string> Reasons);
