using ConsoleShoppingApp.Domain.Models;
using ConsoleShoppingApp.Domain;
using ConsoleShoppingApp.Domain.Repositories;
using ConsoleShoppingApp.Data;
using ConsoleShoppingApp.App.Menus;
using ConsoleShoppingApp.Services;

namespace ConsoleShoppingApp.App;

public sealed class ShoppingApplication
{
    private readonly AuthService _authService;
    private readonly ConsoleView _view;
    private readonly CustomerConsoleFlow _customerFlow;
    private readonly AdminConsoleFlow _adminFlow;

    public ShoppingApplication()
    {
        var store = InMemoryStore.Instance;
        var persistence = new JsonStorePersistence();
        persistence.Load(store);

        var userRepository = new UserRepository(store);
        var productRepository = new ProductRepository(store);
        var orderRepository = new OrderRepository(store);

        _authService = new AuthService(userRepository, store, persistence);
        var catalogService = new CatalogService(productRepository, store, persistence);
        var cartService = new CartService(store, catalogService, persistence);
        var orderService = new OrderService(orderRepository, productRepository, store, catalogService, cartService, new WalletPaymentStrategy(), persistence);

        var orderNotificationLogger = new OrderNotificationLogger();
        orderService.Subscribe(orderNotificationLogger);

        var reviewService = new ReviewService(store, catalogService, orderService, persistence);
        var reportService = new ReportService(store, catalogService);

        _view = new ConsoleView(reviewService);
        _customerFlow = new CustomerConsoleFlow(store, persistence, catalogService, cartService, orderService, reviewService, _view);
        _adminFlow = new AdminConsoleFlow(catalogService, orderService, reportService, _view);

        SeedData.Initialize(store, _authService, catalogService);
        persistence.Save(store);
    }

    public void Run()
    {
        while (true)
        {
            var selection = _view.ShowMenu(ApplicationMenus.CreateMainMenu());
            switch (selection)
            {
                case MainMenuSelection.Register:
                    Register();
                    break;
                case MainMenuSelection.Login:
                    Login();
                    break;
                case MainMenuSelection.Exit:
                    return;
            }
        }
    }

    private void Register()
    {
        Console.Clear();
        Console.WriteLine("=== Register ===");
        Console.WriteLine("1. Customer");
        Console.WriteLine("2. Administrator");

        var role = Input.ReadInt("Select role: ", 1, 2) == 1 ? UserRole.Customer : UserRole.Administrator;
        var username = Input.ReadRequired("Username: ");
        var password = Input.ReadRequired("Password: ");
        var fullName = Input.ReadRequired("Full Name: ");

        try
        {
            _authService.Register(username, password, fullName, role);
            ConsoleView.PrintSuccess("Registration successful.");
        }
        catch (DomainException ex)
        {
            ConsoleView.PrintError(ex.Message);
        }
    }

    private void Login()
    {
        Console.Clear();
        Console.WriteLine("=== Login ===");
        var username = Input.ReadRequired("Username: ");
        var password = Input.ReadRequired("Password: ");

        var user = _authService.Login(username, password);
        if (user is null)
        {
            ConsoleView.PrintError("Invalid username or password.");
            return;
        }

        switch (user)
        {
            case Customer customer:
                _customerFlow.Run(customer);
                break;
            case Administrator administrator:
                _adminFlow.Run(administrator);
                break;
        }
    }
}
