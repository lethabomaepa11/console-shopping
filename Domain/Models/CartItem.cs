namespace ConsoleShoppingApp.Domain.Models;

public sealed class CartItem
{
    public CartItem(Guid productId, int quantity, decimal unitPrice)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Guid ProductId { get; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
