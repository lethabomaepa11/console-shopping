using ConsoleShoppingApp.Domain;
using ConsoleShoppingApp.Domain.Repositories;

namespace ConsoleShoppingApp.Services;

public sealed class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly InMemoryStore _store;
    private readonly IUserFactory _userFactory;
    private readonly IStorePersistence _persistence;

    public AuthService(IUserRepository userRepository, InMemoryStore store, IStorePersistence persistence)
    {
        _userRepository = userRepository;
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

        var exists = _userRepository.GetByUsername(username) != null;
        if (exists)
        {
            throw new DomainException("Username already exists.");
        }

        var user = _userFactory.Create(username.Trim(), password.Trim(), fullName.Trim(), role);
        if (user is Customer customer)
        {
            _userRepository.AddCustomer(customer);
        }
        else if (user is Administrator administrator)
        {
            _userRepository.AddAdministrator(administrator);
        }

        _persistence.Save(_store);
        return user;
    }

    public User? Login(string username, string password)
    {
        var user = _userRepository.GetByUsername(username);
        if (user != null && user.Password == password)
        {
            return user;
        }

        return null;
    }
}
