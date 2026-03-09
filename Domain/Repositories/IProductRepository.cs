using ConsoleShoppingApp.Domain;

namespace ConsoleShoppingApp.Domain.Repositories;

public interface IProductRepository : IRepository<Product>
{
    IEnumerable<Product> Search(string keyword);
    IEnumerable<Product> GetAvailable();
}
