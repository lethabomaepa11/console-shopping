using ConsoleShoppingApp.Domain;
using ConsoleShoppingApp.Domain.Repositories;

namespace ConsoleShoppingApp.Data;

public class UserRepository : IUserRepository
{
    private readonly InMemoryStore _store;

    public UserRepository(InMemoryStore store)
    {
        _store = store;
    }

    public User? GetByUsername(string username)
    {
        var customer = _store.Customers.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        if (customer != null) return customer;
        return _store.Administrators.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    public Customer? GetCustomerById(Guid id) => _store.Customers.FirstOrDefault(c => c.Id == id);

    public Administrator? GetAdminById(Guid id) => _store.Administrators.FirstOrDefault(a => a.Id == id);

    public void AddCustomer(Customer customer) => _store.Customers.Add(customer);

    public void AddAdministrator(Administrator administrator) => _store.Administrators.Add(administrator);

    public IEnumerable<Customer> GetAllCustomers() => _store.Customers;

    public IEnumerable<Administrator> GetAllAdministrators() => _store.Administrators;
}
