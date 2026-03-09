namespace ConsoleShoppingApp.Domain.Models;

public sealed class Review
{
    public Review(Guid productId, Guid customerId, int rating, string comment)
        : this(Guid.NewGuid(), productId, customerId, rating, comment, DateTime.UtcNow)
    {
    }

    public Review(Guid id, Guid productId, Guid customerId, int rating, string comment, DateTime createdAt)
    {
        Id = id;
        ProductId = productId;
        CustomerId = customerId;
        Rating = rating;
        Comment = comment;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }
    public Guid ProductId { get; }
    public Guid CustomerId { get; }
    public int Rating { get; }
    public string Comment { get; }
    public DateTime CreatedAt { get; }
}
