using ConsoleShoppingApp.Domain;
using ConsoleShoppingApp.Domain.Repositories;

namespace ConsoleShoppingApp.Services;

public sealed class CatalogService
{
    private readonly IProductRepository _productRepository;
    private readonly InMemoryStore _store;
    private readonly IStorePersistence _persistence;

    public CatalogService(IProductRepository productRepository, InMemoryStore store, IStorePersistence persistence)
    {
        _productRepository = productRepository;
        _store = store; // Still needed for persistence
        _persistence = persistence;
    }

    public Product AddProduct(string name, string description, string category, decimal price, int stockQuantity)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(category))
        {
            throw new DomainException("Product name and category are required.");
        }

        if (price <= 0 || stockQuantity < 0)
        {
            throw new DomainException("Price must be positive and stock cannot be negative.");
        }

        var product = new Product(name.Trim(), description.Trim(), category.Trim(), price, stockQuantity);
        _productRepository.Add(product);
        _persistence.Save(_store);
        return product;
    }

    public void UpdateProduct(Guid productId, string name, string description, string category, decimal price)
    {
        var product = GetProductOrThrow(productId);
        if (product.IsDeleted)
        {
            throw new DomainException("Cannot update a deleted product.");
        }

        product.Name = name.Trim();
        product.Description = description.Trim();
        product.Category = category.Trim();
        product.Price = price;
        _productRepository.Update(product);
        _persistence.Save(_store);
    }

    public void DeleteProduct(Guid productId)
    {
        var product = GetProductOrThrow(productId);
        _productRepository.Delete(productId);
        _persistence.Save(_store);
    }

    public void RestockProduct(Guid productId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Restock quantity must be greater than 0.");
        }

        var product = GetProductOrThrow(productId);
        product.StockQuantity += quantity;
        _productRepository.Update(product);
        _persistence.Save(_store);
    }

    public List<Product> GetAvailableProducts()
    {
        return _productRepository.GetAvailable()
            .OrderBy(p => p.Name)
            .ToList();
    }

    public List<Product> GetAllProducts()
    {
        return _productRepository.GetAll()
            .OrderBy(p => p.Name)
            .ToList();
    }

    public List<Product> SearchProducts(string keyword)
    {
        return _productRepository.Search(keyword)
            .OrderBy(p => p.Name)
            .ToList();
    }

    public Product GetProductOrThrow(Guid productId)
    {
        var product = _productRepository.GetById(productId);
        if (product is null)
        {
            throw new DomainException("Product not found.");
        }

        return product;
    }
}
