using ConsoleShoppingApp.Domain;

namespace ConsoleShoppingApp.Services;

public sealed class ReviewService
{
    private readonly InMemoryStore _store;
    private readonly CatalogService _catalogService;
    private readonly OrderService _orderService;
    private readonly IStorePersistence _persistence;

    public ReviewService(InMemoryStore store, CatalogService catalogService, OrderService orderService, IStorePersistence persistence)
    {
        _store = store;
        _catalogService = catalogService;
        _orderService = orderService;
        _persistence = persistence;
    }

    public void AddReview(Guid customerId, Guid productId, int rating, string comment)
    {
        _catalogService.GetProductOrThrow(productId);
        if (rating is < 1 or > 5)
        {
            throw new DomainException("Rating must be between 1 and 5.");
        }

        if (!_orderService.HasCustomerPurchasedProduct(customerId, productId))
        {
            throw new DomainException("Only customers who purchased the product can review it.");
        }

        var existing = _store.Reviews.FirstOrDefault(r => r.CustomerId == customerId && r.ProductId == productId);
        if (existing is not null)
        {
            throw new DomainException("You have already reviewed this product.");
        }

        _store.Reviews.Add(new Review(productId, customerId, rating, comment.Trim()));
        _persistence.Save(_store);
    }

    public double GetAverageRating(Guid productId)
    {
        var ratings = _store.Reviews
            .Where(r => r.ProductId == productId)
            .Select(r => r.Rating)
            .ToList();

        return ratings.Any() ? ratings.Average() : 0.0;
    }
}
