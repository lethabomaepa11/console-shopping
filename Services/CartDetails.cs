namespace ConsoleShoppingApp.Services;

public sealed record CartDetails(List<CartItemDetails> Items, decimal Total);

public sealed record CartItemDetails(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);
