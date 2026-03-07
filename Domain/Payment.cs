namespace ConsoleShoppingApp.Domain;

public sealed class Payment
{
    public Payment(Guid orderId, decimal amount, PaymentMethod method, PaymentStatus status)
        : this(Guid.NewGuid(), orderId, amount, method, status, DateTime.UtcNow)
    {
    }

    public Payment(Guid id, Guid orderId, decimal amount, PaymentMethod method, PaymentStatus status, DateTime paidAt)
    {
        Id = id;
        OrderId = orderId;
        Amount = amount;
        Method = method;
        Status = status;
        PaidAt = paidAt;
    }

    public Guid Id { get; }
    public Guid OrderId { get; }
    public decimal Amount { get; }
    public PaymentMethod Method { get; }
    public PaymentStatus Status { get; }
    public DateTime PaidAt { get; }
}
