using ConsoleShoppingApp.App.Menus;
using ConsoleShoppingApp.Domain;
using ConsoleShoppingApp.Services;

namespace ConsoleShoppingApp.App;

public sealed class AdminConsoleFlow
{
    private readonly CatalogService _catalogService;
    private readonly OrderService _orderService;
    private readonly ReportService _reportService;
    private readonly ConsoleView _view;

    public AdminConsoleFlow(
        CatalogService catalogService,
        OrderService orderService,
        ReportService reportService,
        ConsoleView view)
    {
        _catalogService = catalogService;
        _orderService = orderService;
        _reportService = reportService;
        _view = view;
    }

    public void Run(Administrator admin)
    {
        AccessGuard.EnsureAdministrator(admin);

        while (true)
        {
            var selection = _view.ShowMenu(ApplicationMenus.CreateAdministratorMenu(admin.FullName));
            if (!HandleSelection(selection))
            {
                return;
            }
        }
    }

    private bool HandleSelection(AdminMenuSelection selection)
    {
        switch (selection)
        {
            case AdminMenuSelection.AddProduct:
                AddProduct();
                return true;
            case AdminMenuSelection.UpdateProduct:
                UpdateProduct();
                return true;
            case AdminMenuSelection.DeleteProduct:
                DeleteProduct();
                return true;
            case AdminMenuSelection.RestockProduct:
                RestockProduct();
                return true;
            case AdminMenuSelection.ViewProducts:
                _view.ShowProductsScreen("=== Product Catalog ===", _catalogService.GetAvailableProducts());
                return true;
            case AdminMenuSelection.ViewOrders:
                ViewAllOrders();
                return true;
            case AdminMenuSelection.UpdateOrderStatus:
                UpdateOrderStatus();
                return true;
            case AdminMenuSelection.ViewLowStockProducts:
                ViewLowStock();
                return true;
            case AdminMenuSelection.GenerateSalesReports:
                ShowReports();
                return true;
            case AdminMenuSelection.Logout:
                return false;
            default:
                throw new ArgumentOutOfRangeException(nameof(selection), selection, null);
        }
    }

    private void AddProduct()
    {
        Console.Clear();
        var name = Input.ReadRequired("Name: ");
        var description = Input.ReadRequired("Description: ");
        var category = Input.ReadRequired("Category: ");
        var price = Input.ReadDecimal("Price: ", 0.01m, 1_000_000m);
        var stock = Input.ReadInt("Stock Quantity: ", 0, 1_000_000);

        try
        {
            _catalogService.AddProduct(name, description, category, price, stock);
            ConsoleView.PrintSuccess("Product added.");
        }
        catch (DomainException ex)
        {
            ConsoleView.PrintError(ex.Message);
        }
    }

    private void UpdateProduct()
    {
        Console.Clear();
        var products = _catalogService.GetAllProducts();
        Console.WriteLine("=== Update Product ===");
        _view.PrintProductList(products);
        if (!products.Any())
        {
            ConsoleView.Pause();
            return;
        }

        var selected = ConsoleView.SelectProduct(products, "Select product number: ");
        var name = Input.ReadRequired("New Name: ");
        var description = Input.ReadRequired("New Description: ");
        var category = Input.ReadRequired("New Category: ");
        var price = Input.ReadDecimal("New Price: ", 0.01m, 1_000_000m);

        try
        {
            _catalogService.UpdateProduct(selected.Id, name, description, category, price);
            ConsoleView.PrintSuccess("Product updated.");
        }
        catch (DomainException ex)
        {
            ConsoleView.PrintError(ex.Message);
        }
    }

    private void DeleteProduct()
    {
        Console.Clear();
        var products = _catalogService.GetAllProducts();
        Console.WriteLine("=== Delete Product ===");
        _view.PrintProductList(products);
        if (!products.Any())
        {
            ConsoleView.Pause();
            return;
        }

        var selected = ConsoleView.SelectProduct(products, "Select product number: ");
        try
        {
            _catalogService.DeleteProduct(selected.Id);
            ConsoleView.PrintSuccess("Product deleted.");
        }
        catch (DomainException ex)
        {
            ConsoleView.PrintError(ex.Message);
        }
    }

    private void RestockProduct()
    {
        Console.Clear();
        var products = _catalogService.GetAllProducts();
        Console.WriteLine("=== Restock Product ===");
        _view.PrintProductList(products);
        if (!products.Any())
        {
            ConsoleView.Pause();
            return;
        }

        var selected = ConsoleView.SelectProduct(products, "Select product number: ");
        var quantity = Input.ReadInt("Quantity to add: ", 1, 1_000_000);
        try
        {
            _catalogService.RestockProduct(selected.Id, quantity);
            ConsoleView.PrintSuccess("Stock updated.");
        }
        catch (DomainException ex)
        {
            ConsoleView.PrintError(ex.Message);
        }
    }

    private void ViewAllOrders()
    {
        Console.Clear();
        Console.WriteLine("=== All Orders ===");
        _view.PrintOrderList(_orderService.GetAllOrders());
        ConsoleView.Pause();
    }

    private void UpdateOrderStatus()
    {
        Console.Clear();
        var orders = _orderService.GetAllOrders();
        Console.WriteLine("=== Update Order Status ===");
        _view.PrintOrderList(orders);
        if (!orders.Any())
        {
            ConsoleView.Pause();
            return;
        }

        var selectedOrder = ConsoleView.SelectOrder(orders, "Select order number: ");

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
            ConsoleView.PrintSuccess("Order status updated.");
        }
        catch (DomainException ex)
        {
            ConsoleView.PrintError(ex.Message);
        }
    }

    private void ViewLowStock()
    {
        Console.Clear();
        Console.WriteLine("=== Low Stock Products ===");
        _view.PrintProductList(_reportService.GetLowStockProducts(5));
        ConsoleView.Pause();
    }

    private void ShowReports()
    {
        Console.Clear();
        var report = _reportService.GenerateSalesReport();
        Console.WriteLine("=== Sales Report ===");
        Console.WriteLine($"Total Orders: {report.TotalOrders}");
        Console.WriteLine($"Total Revenue: {report.TotalRevenue:C2}");
        Console.WriteLine($"Average Order Value: {report.AverageOrderValue:C2}");
        Console.WriteLine($"Top Product: {report.TopProductName}");
        Console.WriteLine("Top Selling Products:");

        report.TopProducts
            .Select(item => $"- {item.ProductName}: {item.QuantitySold} units")
            .ToList()
            .ForEach(Console.WriteLine);

        ConsoleView.Pause();
    }
}
