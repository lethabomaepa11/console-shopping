using ConsoleShoppingApp.Domain;

namespace ConsoleShoppingApp.Domain.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    IEnumerable<Order> GetByCustomerId(Guid customerId);
    bool HasPurchasedProduct(Guid customerId, Guid productId);
}
