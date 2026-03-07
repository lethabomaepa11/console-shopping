using ConsoleShoppingApp.Domain;

namespace ConsoleShoppingApp.Services;

public sealed class CartService
{
    private readonly InMemoryStore _store;
    private readonly CatalogService _catalogService;
    private readonly IStorePersistence _persistence;

    public CartService(InMemoryStore store, CatalogService catalogService, IStorePersistence persistence)
    {
        _store = store;
        _catalogService = catalogService;
        _persistence = persistence;
    }

    public void AddToCart(Guid customerId, Guid productId, int quantity)
    {
        var customer = GetCustomerOrThrow(customerId);
        var product = _catalogService.GetProductOrThrow(productId);

        if (product.IsDeleted || product.StockQuantity <= 0)
        {
            throw new DomainException("Product is not available.");
        }

        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be greater than 0.");
        }

        var existing = customer.Cart.Items.FirstOrDefault(i => i.ProductId == productId);
        var requestedQuantity = (existing?.Quantity ?? 0) + quantity;
        if (requestedQuantity > product.StockQuantity)
        {
            throw new DomainException("Requested quantity exceeds stock.");
        }

        if (existing is null)
        {
            customer.Cart.Items.Add(new CartItem(product.Id, quantity, product.Price));
            _persistence.Save(_store);
            return;
        }

        existing.Quantity = requestedQuantity;
        existing.UnitPrice = product.Price;
        _persistence.Save(_store);
    }

    public void UpdateQuantity(Guid customerId, Guid productId, int quantity)
    {
        var customer = GetCustomerOrThrow(customerId);
        var item = customer.Cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
        {
            throw new DomainException("Product is not in cart.");
        }

        if (quantity == 0)
        {
            customer.Cart.Items.Remove(item);
            _persistence.Save(_store);
            return;
        }

        var product = _catalogService.GetProductOrThrow(productId);
        if (quantity > product.StockQuantity)
        {
            throw new DomainException("Requested quantity exceeds stock.");
        }

        item.Quantity = quantity;
        item.UnitPrice = product.Price;
        _persistence.Save(_store);
    }

    public CartDetails GetCartDetails(Guid customerId)
    {
        var customer = GetCustomerOrThrow(customerId);
        var items = customer.Cart.Items
            .Select(item =>
            {
                var product = _store.Products.FirstOrDefault(p => p.Id == item.ProductId);
                var name = product?.Name ?? "Unknown Product";
                var lineTotal = item.Quantity * item.UnitPrice;
                return new CartItemDetails(item.ProductId, name, item.Quantity, item.UnitPrice, lineTotal);
            })
            .ToList();

        var total = items.Sum(i => i.LineTotal);
        return new CartDetails(items, total);
    }

    public void ClearCart(Guid customerId)
    {
        var customer = GetCustomerOrThrow(customerId);
        customer.Cart.Items.Clear();
        _persistence.Save(_store);
    }

    public Customer GetCustomerOrThrow(Guid customerId)
    {
        var customer = _store.Customers.FirstOrDefault(c => c.Id == customerId);
        if (customer is null)
        {
            throw new DomainException("Customer not found.");
        }

        return customer;
    }
}
