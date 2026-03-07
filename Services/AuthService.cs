using ConsoleShoppingApp.Domain;

namespace ConsoleShoppingApp.Services;

public sealed class AuthService
{
    private readonly InMemoryStore _store;
    private readonly IUserFactory _userFactory;
    private readonly IStorePersistence _persistence;

    public AuthService(InMemoryStore store, IStorePersistence persistence)
    {
        _store = store;
        _persistence = persistence;
        _userFactory = new UserFactory();
    }

    public User Register(string username, string password, string fullName, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fullName))
        {
            throw new DomainException("All fields are required.");
        }

        var exists = _store.Customers.Any(c => c.Username.Equals(username, StringComparison.OrdinalIgnoreCase)) ||
                     _store.Administrators.Any(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        if (exists)
        {
            throw new DomainException("Username already exists.");
        }

        var user = _userFactory.Create(username.Trim(), password.Trim(), fullName.Trim(), role);
        if (user is Customer customer)
        {
            _store.Customers.Add(customer);
        }
        else if (user is Administrator administrator)
        {
            _store.Administrators.Add(administrator);
        }

        _persistence.Save(_store);
        return user;
    }

    public User? Login(string username, string password)
    {
        var user = _store.Customers.Cast<User>()
            .Concat(_store.Administrators)
            .FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);

        return user;
    }
}
