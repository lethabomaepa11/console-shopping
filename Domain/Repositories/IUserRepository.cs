using ConsoleShoppingApp.Domain;

namespace ConsoleShoppingApp.Domain.Repositories;

public interface IUserRepository
{
    User? GetByUsername(string username);
    Customer? GetCustomerById(Guid id);
    Administrator? GetAdminById(Guid id);
    void AddCustomer(Customer customer);
    void AddAdministrator(Administrator administrator);
    IEnumerable<Customer> GetAllCustomers();
    IEnumerable<Administrator> GetAllAdministrators();
}
