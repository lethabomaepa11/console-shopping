using ConsoleShoppingApp.Domain;
using ConsoleShoppingApp.Domain.Repositories;

namespace ConsoleShoppingApp.Data;

public class OrderRepository : IOrderRepository
{
    private readonly InMemoryStore _store;

    public OrderRepository(InMemoryStore store)
    {
        _store = store;
    }

    public Order? GetById(Guid id) => _store.Orders.FirstOrDefault(o => o.Id == id);

    public IEnumerable<Order> GetAll() => _store.Orders;

    public void Add(Order entity) => _store.Orders.Add(entity);

    public void Update(Order entity)
    {
        var index = _store.Orders.FindIndex(o => o.Id == entity.Id);
        if (index != -1)
        {
            _store.Orders[index] = entity;
        }
    }

    public void Delete(Guid id)
    {
        var order = GetById(id);
        if (order != null)
        {
            _store.Orders.Remove(order);
        }
    }

    public IEnumerable<Order> GetByCustomerId(Guid customerId) => _store.Orders.Where(o => o.CustomerId == customerId);

    public bool HasPurchasedProduct(Guid customerId, Guid productId)
    {
        return _store.Orders.Any(o =>
            o.CustomerId == customerId &&
            o.Status != OrderStatus.Cancelled &&
            o.Items.Any(i => i.ProductId == productId));
    }
}
