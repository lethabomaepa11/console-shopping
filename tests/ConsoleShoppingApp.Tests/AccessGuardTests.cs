using ConsoleShoppingApp.App;
using ConsoleShoppingApp.Domain.Models;
using ConsoleShoppingApp.Domain;

namespace ConsoleShoppingApp.Tests;

internal static class AccessGuardTests
{
    public static void Run()
    {
        EnsureCustomer_AllowsCustomers();
        EnsureCustomer_RejectsAdministrators();
        EnsureAdministrator_AllowsAdministrators();
        EnsureAdministrator_RejectsCustomers();
    }

    private static void EnsureCustomer_AllowsCustomers()
    {
        var customer = new Customer("customer", "password", "Customer User");
        AccessGuard.EnsureCustomer(customer);
    }

    private static void EnsureCustomer_RejectsAdministrators()
    {
        var admin = new Administrator("admin", "password", "Administrator User");

        TestAssert.Throws<DomainException>(
            () => AccessGuard.EnsureCustomer(admin),
            "Access denied. Customer role required.");
    }

    private static void EnsureAdministrator_AllowsAdministrators()
    {
        var admin = new Administrator("admin", "password", "Administrator User");
        AccessGuard.EnsureAdministrator(admin);
    }

    private static void EnsureAdministrator_RejectsCustomers()
    {
        var customer = new Customer("customer", "password", "Customer User");

        TestAssert.Throws<DomainException>(
            () => AccessGuard.EnsureAdministrator(customer),
            "Access denied. Administrator role required.");
    }
}
