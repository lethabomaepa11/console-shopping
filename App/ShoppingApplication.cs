using ConsoleShoppingApp.Domain;
using ConsoleShoppingApp.Domain.Repositories;
using ConsoleShoppingApp.Data;
using ConsoleShoppingApp.App.Menus;
using ConsoleShoppingApp.Services;

namespace ConsoleShoppingApp.App;

public sealed class ShoppingApplication
{
    private readonly InMemoryStore _store;
    private readonly IStorePersistence _persistence;
    private readonly AuthService _authService;
    private readonly CatalogService _catalogService;
    private readonly CartService _cartService;
    private readonly OrderService _orderService;
    private readonly ReviewService _reviewService;
    private readonly ReportService _reportService;

    public ShoppingApplication()
    {
        _store = InMemoryStore.Instance;
        _persistence = new JsonStorePersistence();
        _persistence.Load(_store);

        var userRepository = new UserRepository(_store);
        var productRepository = new ProductRepository(_store);
        var orderRepository = new OrderRepository(_store);

        _authService = new AuthService(userRepository, _store, _persistence);
        _catalogService = new CatalogService(productRepository, _store, _persistence);
        _cartService = new CartService(_store, _catalogService, _persistence);
        _orderService = new OrderService(orderRepository, productRepository, _store, _catalogService, _cartService, new WalletPaymentStrategy(), _persistence);

        var orderNotificationLogger = new OrderNotificationLogger();
        _orderService.Subscribe(orderNotificationLogger);

        _reviewService = new ReviewService(_store, _catalogService, _orderService, _persistence);
        _reportService = new ReportService(_store, _catalogService);

        SeedData.Initialize(_store, _authService, _catalogService);
        _persistence.Save(_store);
    }

    public void Run()
    {
        while (true)
        {
            var selection = ShowMenu(ApplicationMenus.CreateMainMenu());
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
            PrintSuccess("Registration successful.");
        }
        catch (DomainException ex)
        {
            PrintError(ex.Message);
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
            PrintError("Invalid username or password.");
            return;
        }

        if (user is Customer customer)
        {
            CustomerMenu(customer);
        }
        else if (user is Administrator administrator)
        {
            AdminMenu(administrator);
        }
    }

    private void CustomerMenu(Customer customer)
    {
        AccessGuard.EnsureCustomer(customer);

        while (true)
        {
            var selection = ShowMenu(ApplicationMenus.CreateCustomerMenu(customer.FullName));
            if (!HandleCustomerMenuSelection(customer, selection))
            {
                return;
            }
        }
    }

    private void AdminMenu(Administrator admin)
    {
        AccessGuard.EnsureAdministrator(admin);

        while (true)
        {
            var selection = ShowMenu(ApplicationMenus.CreateAdministratorMenu(admin.FullName));
            if (!HandleAdministratorMenuSelection(admin, selection))
            {
                return;
            }
        }
    }

    private bool HandleCustomerMenuSelection(Customer customer, CustomerMenuSelection selection)
    {
        switch (selection)
        {
            case CustomerMenuSelection.BrowseProducts:
                BrowseProducts();
                return true;
            case CustomerMenuSelection.SearchProducts:
                SearchProducts();
                return true;
            case CustomerMenuSelection.AddProductToCart:
                AddToCart(customer);
                return true;
            case CustomerMenuSelection.ViewCart:
                ViewCart(customer);
                return true;
            case CustomerMenuSelection.UpdateCart:
                UpdateCart(customer);
                return true;
            case CustomerMenuSelection.Checkout:
                Checkout(customer);
                return true;
            case CustomerMenuSelection.ViewWalletBalance:
                PrintInfo($"Wallet Balance: {customer.WalletBalance:C2}");
                return true;
            case CustomerMenuSelection.AddWalletFunds:
                AddFunds(customer);
                return true;
            case CustomerMenuSelection.ViewOrderHistory:
                ViewOrderHistory(customer);
                return true;
            case CustomerMenuSelection.TrackOrders:
                TrackOrders(customer);
                return true;
            case CustomerMenuSelection.ReviewProducts:
                ReviewProduct(customer);
                return true;
            case CustomerMenuSelection.Logout:
                return false;
            default:
                throw new ArgumentOutOfRangeException(nameof(selection), selection, null);
        }
    }

    private bool HandleAdministratorMenuSelection(Administrator admin, AdminMenuSelection selection)
    {
        switch (selection)
        {
            case AdminMenuSelection.AddProduct:
                AddProduct(admin);
                return true;
            case AdminMenuSelection.UpdateProduct:
                UpdateProduct(admin);
                return true;
            case AdminMenuSelection.DeleteProduct:
                DeleteProduct(admin);
                return true;
            case AdminMenuSelection.RestockProduct:
                RestockProduct(admin);
                return true;
            case AdminMenuSelection.ViewProducts:
                BrowseProducts();
                return true;
            case AdminMenuSelection.ViewOrders:
                ViewAllOrders(admin);
                return true;
            case AdminMenuSelection.UpdateOrderStatus:
                UpdateOrderStatus(admin);
                return true;
            case AdminMenuSelection.ViewLowStockProducts:
                ViewLowStock(admin);
                return true;
            case AdminMenuSelection.GenerateSalesReports:
                ShowReports(admin);
                return true;
            case AdminMenuSelection.Logout:
                return false;
            default:
                throw new ArgumentOutOfRangeException(nameof(selection), selection, null);
        }
    }

    private void BrowseProducts()
    {
        Console.Clear();
        Console.WriteLine("=== Product Catalog ===");
        PrintProductList(_catalogService.GetAvailableProducts());
        Pause();
    }

    private void SearchProducts()
    {
        Console.Clear();
        var keyword = Input.ReadRequired("Search keyword: ");
        var products = _catalogService.SearchProducts(keyword);
        Console.WriteLine("=== Search Results ===");
        PrintProductList(products);
        Pause();
    }

    private void AddToCart(Customer customer)
    {
        AccessGuard.EnsureCustomer(customer);
        Console.Clear();
        var products = _catalogService.GetAvailableProducts();
        Console.WriteLine("=== Add Product to Cart ===");
        PrintProductList(products);

        if (!products.Any())
        {
            Pause();
            return;
        }

        var selected = SelectProduct(products, "Select product number: ");
        var quantity = Input.ReadInt("Quantity: ", 1, 1000);

        try
        {
            _cartService.AddToCart(customer.Id, selected.Id, quantity);
            PrintSuccess("Product added to cart.");
        }
        catch (DomainException ex)
        {
            PrintError(ex.Message);
        }
    }

    private void ViewCart(Customer customer)
    {
        AccessGuard.EnsureCustomer(customer);
        Console.Clear();
        Console.WriteLine("=== Cart ===");
        var details = _cartService.GetCartDetails(customer.Id);
        if (!details.Items.Any())
        {
            Console.WriteLine("Cart is empty.");
        }
        else
        {
            for (var i = 0; i < details.Items.Count; i++)
            {
                var item = details.Items[i];
                Console.WriteLine($"{i + 1}. {item.ProductName} | Qty: {item.Quantity} | Unit: {item.UnitPrice:C2} | Line: {item.LineTotal:C2}");
            }

            Console.WriteLine($"Total: {details.Total:C2}");
        }

        Pause();
    }

    private void UpdateCart(Customer customer)
    {
        AccessGuard.EnsureCustomer(customer);
        Console.Clear();
        Console.WriteLine("=== Update Cart ===");
        var details = _cartService.GetCartDetails(customer.Id);
        if (!details.Items.Any())
        {
            Console.WriteLine("Cart is empty.");
            Pause();
            return;
        }

        for (var i = 0; i < details.Items.Count; i++)
        {
            var item = details.Items[i];
            Console.WriteLine($"{i + 1}. {item.ProductName} | Qty: {item.Quantity} | Unit: {item.UnitPrice:C2} | Line: {item.LineTotal:C2}");
        }

        var selectedIndex = Input.ReadInt("Select item number: ", 1, details.Items.Count) - 1;
        var selected = details.Items[selectedIndex];
        var quantity = Input.ReadInt("New quantity (0 removes): ", 0, 1000);

        try
        {
            _cartService.UpdateQuantity(customer.Id, selected.ProductId, quantity);
            PrintSuccess("Cart updated.");
        }
        catch (DomainException ex)
        {
            PrintError(ex.Message);
        }
    }

    private void Checkout(Customer customer)
    {
        AccessGuard.EnsureCustomer(customer);
        Console.Clear();
        try
        {
            var order = _orderService.Checkout(customer.Id);
            PrintSuccess($"Checkout successful. Total: {order.TotalAmount:C2}");
        }
        catch (DomainException ex)
        {
            PrintError(ex.Message);
        }
    }

    private void AddFunds(Customer customer)
    {
        AccessGuard.EnsureCustomer(customer);
        Console.Clear();
        var amount = Input.ReadDecimal("Amount to add: ", 1m, 1_000_000m);
        customer.WalletBalance += amount;
        _persistence.Save(_store);
        PrintSuccess($"Funds added. New balance: {customer.WalletBalance:C2}");
    }

    private void ViewOrderHistory(Customer customer)
    {
        AccessGuard.EnsureCustomer(customer);
        Console.Clear();
        Console.WriteLine("=== Order History ===");
        PrintOrderList(_orderService.GetCustomerOrders(customer.Id));
        Pause();
    }

    private void TrackOrders(Customer customer)
    {
        AccessGuard.EnsureCustomer(customer);
        Console.Clear();
        Console.WriteLine("=== Track Orders ===");
        var orders = _orderService.GetCustomerOrders(customer.Id);
        PrintOrderList(orders);
        if (!orders.Any())
        {
            Pause();
            return;
        }

        var selectedOrder = SelectOrder(orders, "Select order number: ");
        PrintInfo($"Order status: {selectedOrder.Status}");
    }

    private void ReviewProduct(Customer customer)
    {
        AccessGuard.EnsureCustomer(customer);
        Console.Clear();
        var products = _catalogService.GetAvailableProducts();
        Console.WriteLine("=== Review Products ===");
        PrintProductList(products);
        if (!products.Any())
        {
            Pause();
            return;
        }

        var selected = SelectProduct(products, "Select product number: ");
        var rating = Input.ReadInt("Rating (1-5): ", 1, 5);
        var comment = Input.ReadRequired("Comment: ");

        try
        {
            _reviewService.AddReview(customer.Id, selected.Id, rating, comment);
            PrintSuccess("Review submitted.");
        }
        catch (DomainException ex)
        {
            PrintError(ex.Message);
        }
    }

    private void AddProduct(Administrator admin)
    {
        AccessGuard.EnsureAdministrator(admin);
        Console.Clear();
        var name = Input.ReadRequired("Name: ");
        var description = Input.ReadRequired("Description: ");
        var category = Input.ReadRequired("Category: ");
        var price = Input.ReadDecimal("Price: ", 0.01m, 1_000_000m);
        var stock = Input.ReadInt("Stock Quantity: ", 0, 1_000_000);

        try
        {
            _catalogService.AddProduct(name, description, category, price, stock);
            PrintSuccess("Product added.");
        }
        catch (DomainException ex)
        {
            PrintError(ex.Message);
        }
    }

    private void UpdateProduct(Administrator admin)
    {
        AccessGuard.EnsureAdministrator(admin);
        Console.Clear();
        var products = _catalogService.GetAllProducts();
        Console.WriteLine("=== Update Product ===");
        PrintProductList(products);
        if (!products.Any())
        {
            Pause();
            return;
        }

        var selected = SelectProduct(products, "Select product number: ");
        var name = Input.ReadRequired("New Name: ");
        var description = Input.ReadRequired("New Description: ");
        var category = Input.ReadRequired("New Category: ");
        var price = Input.ReadDecimal("New Price: ", 0.01m, 1_000_000m);

        try
        {
            _catalogService.UpdateProduct(selected.Id, name, description, category, price);
            PrintSuccess("Product updated.");
        }
        catch (DomainException ex)
        {
            PrintError(ex.Message);
        }
    }

    private void DeleteProduct(Administrator admin)
    {
        AccessGuard.EnsureAdministrator(admin);
        Console.Clear();
        var products = _catalogService.GetAllProducts();
        Console.WriteLine("=== Delete Product ===");
        PrintProductList(products);
        if (!products.Any())
        {
            Pause();
            return;
        }

        var selected = SelectProduct(products, "Select product number: ");
        try
        {
            _catalogService.DeleteProduct(selected.Id);
            PrintSuccess("Product deleted.");
        }
        catch (DomainException ex)
        {
            PrintError(ex.Message);
        }
    }

    private void RestockProduct(Administrator admin)
    {
        AccessGuard.EnsureAdministrator(admin);
        Console.Clear();
        var products = _catalogService.GetAllProducts();
        Console.WriteLine("=== Restock Product ===");
        PrintProductList(products);
        if (!products.Any())
        {
            Pause();
            return;
        }

        var selected = SelectProduct(products, "Select product number: ");
        var quantity = Input.ReadInt("Quantity to add: ", 1, 1_000_000);
        try
        {
            _catalogService.RestockProduct(selected.Id, quantity);
            PrintSuccess("Stock updated.");
        }
        catch (DomainException ex)
        {
            PrintError(ex.Message);
        }
    }

    private void ViewAllOrders(Administrator admin)
    {
        AccessGuard.EnsureAdministrator(admin);
        Console.Clear();
        Console.WriteLine("=== All Orders ===");
        PrintOrderList(_orderService.GetAllOrders());
        Pause();
    }

    private void UpdateOrderStatus(Administrator admin)
    {
        AccessGuard.EnsureAdministrator(admin);
        Console.Clear();
        var orders = _orderService.GetAllOrders();
        Console.WriteLine("=== Update Order Status ===");
        PrintOrderList(orders);
        if (!orders.Any())
        {
            Pause();
            return;
        }

        var selectedOrder = SelectOrder(orders, "Select order number: ");

        Console.WriteLine("1. Pending");
        Console.WriteLine("2. Paid");
        Console.WriteLine("3. Processing");
        Console.WriteLine("4. Shipped");
        Console.WriteLine("5. Delivered");
        Console.WriteLine("6. Cancelled");
        var choice = Input.ReadInt("New status: ", 1, 6);
        var status = (OrderStatus)(choice - 1);

        try
        {
            _orderService.UpdateOrderStatus(selectedOrder.Id, status);
            PrintSuccess("Order status updated.");
        }
        catch (DomainException ex)
        {
            PrintError(ex.Message);
        }
    }

    private void ViewLowStock(Administrator admin)
    {
        AccessGuard.EnsureAdministrator(admin);
        Console.Clear();
        Console.WriteLine("=== Low Stock Products ===");
        PrintProductList(_reportService.GetLowStockProducts(5));
        Pause();
    }

    private void ShowReports(Administrator admin)
    {
        AccessGuard.EnsureAdministrator(admin);
        Console.Clear();
        var report = _reportService.GenerateSalesReport();
        Console.WriteLine("=== Sales Report ===");
        Console.WriteLine($"Total Orders: {report.TotalOrders}");
        Console.WriteLine($"Total Revenue: {report.TotalRevenue:C2}");
        Console.WriteLine($"Average Order Value: {report.AverageOrderValue:C2}");
        Console.WriteLine($"Top Product: {report.TopProductName}");
        Console.WriteLine("Top Selling Products:");
        foreach (var item in report.TopProducts)
        {
            Console.WriteLine($"- {item.ProductName}: {item.QuantitySold} units");
        }

        Pause();
    }

    private void PrintProductList(IReadOnlyList<Product> products)
    {
        if (!products.Any())
        {
            Console.WriteLine("No products found.");
            return;
        }

        for (var i = 0; i < products.Count; i++)
        {
            var product = products[i];
            var avgRating = _reviewService.GetAverageRating(product.Id);
            Console.WriteLine($"{i + 1}. {product.Name} | {product.Category} | {product.Price:C2} | Stock: {product.StockQuantity} | Rating: {avgRating:F1}");
        }
    }

    private static void PrintOrderList(IReadOnlyList<Order> orders)
    {
        if (!orders.Any())
        {
            Console.WriteLine("No orders found.");
            return;
        }

        for (var i = 0; i < orders.Count; i++)
        {
            var order = orders[i];
            Console.WriteLine($"{i + 1}. {order.CreatedAt:u} | Total: {order.TotalAmount:C2} | Status: {order.Status}");
        }
    }

    private static Product SelectProduct(IReadOnlyList<Product> products, string prompt)
    {
        var index = Input.ReadInt(prompt, 1, products.Count) - 1;
        return products[index];
    }

    private static Order SelectOrder(IReadOnlyList<Order> orders, string prompt)
    {
        var index = Input.ReadInt(prompt, 1, orders.Count) - 1;
        return orders[index];
    }

    private static TSelection ShowMenu<TSelection>(MenuDefinition<TSelection> menu) where TSelection : struct, Enum
    {
        Console.Clear();
        Console.WriteLine(menu.Title);
        foreach (var option in menu.Options)
        {
            Console.WriteLine($"{option.Number}. {option.Label}");
        }

        var selectedIndex = Input.ReadInt(menu.Prompt, 1, menu.Options.Count) - 1;
        return menu.Options[selectedIndex].Selection;
    }

    private static void PrintSuccess(string message)
    {
        Console.WriteLine(message);
        Pause();
    }

    private static void PrintError(string message)
    {
        Console.WriteLine($"Error: {message}");
        Pause();
    }

    private static void PrintInfo(string message)
    {
        Console.WriteLine(message);
        Pause();
    }

    private static void Pause()
    {
        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }
}
