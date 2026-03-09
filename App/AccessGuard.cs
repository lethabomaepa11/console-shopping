using ConsoleShoppingApp.Domain;

namespace ConsoleShoppingApp.App;

public static class AccessGuard
{
    public static void EnsureCustomer(User user)
    {
        if (user.Role != UserRole.Customer)
        {
            throw new DomainException("Access denied. Customer role required.");
        }
    }

    public static void EnsureAdministrator(User user)
    {
        if (user.Role != UserRole.Administrator)
        {
            throw new DomainException("Access denied. Administrator role required.");
        }
    }
}
