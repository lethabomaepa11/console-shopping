using ConsoleShoppingApp.Domain.Models;

namespace ConsoleShoppingApp.Domain;

public interface IUserFactory
{
    User Create(string username, string password, string fullName, UserRole role);
}

public sealed class UserFactory : IUserFactory
{
    public User Create(string username, string password, string fullName, UserRole role)
    {
        return role switch
        {
            UserRole.Customer => new Customer(username, password, fullName),
            UserRole.Administrator => new Administrator(username, password, fullName),
            _ => throw new DomainException("Unsupported role.")
        };
    }
}
