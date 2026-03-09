using ConsoleShoppingApp.Domain.Models;
using ConsoleShoppingApp.Domain;
using ConsoleShoppingApp.Domain.Repositories;

namespace ConsoleShoppingApp.Data;

public class ProductRepository : IProductRepository
{
    private readonly InMemoryStore _store;

    public ProductRepository(InMemoryStore store)
    {
        _store = store;
    }

    public Product? GetById(Guid id) => _store.Products.FirstOrDefault(p => p.Id == id);

    public IEnumerable<Product> GetAll() => _store.Products.Where(p => !p.IsDeleted);

    public void Add(Product entity) => _store.Products.Add(entity);

    public void Update(Product entity)
    {
        var existing = GetById(entity.Id);
        if (existing != null && existing != entity)
        {
            // In-memory store uses references, so if it's already in the list, it's updated.
            // If it's a different object, we might need to replace it.
            var index = _store.Products.FindIndex(p => p.Id == entity.Id);
            if (index != -1)
            {
                _store.Products[index] = entity;
            }
        }
    }

    public void Delete(Guid id)
    {
        var product = GetById(id);
        if (product != null)
        {
            product.IsDeleted = true;
        }
    }

    public IEnumerable<Product> Search(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return GetAvailable();
        var key = keyword.Trim().ToLowerInvariant();
        return _store.Products
            .Where(p => !p.IsDeleted && p.StockQuantity > 0)
            .Where(p =>
                p.Name.ToLowerInvariant().Contains(key) ||
                p.Description.ToLowerInvariant().Contains(key) ||
                p.Category.ToLowerInvariant().Contains(key));
    }

    public IEnumerable<Product> GetAvailable() => _store.Products.Where(p => !p.IsDeleted && p.StockQuantity > 0);
}
