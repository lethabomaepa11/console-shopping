using ConsoleShoppingApp.App.Menus;
using ConsoleShoppingApp.Domain.Models;
using ConsoleShoppingApp.Domain;
using ConsoleShoppingApp.Services;

namespace ConsoleShoppingApp.App;

public sealed class CustomerConsoleFlow
{
    private readonly InMemoryStore _store;
    private readonly IStorePersistence _persistence;
    private readonly CatalogService _catalogService;
    private readonly CartService _cartService;
    private readonly OrderService _orderService;
    private readonly ReviewService _reviewService;
    private readonly RecommendationService _recommendationService;
    private readonly IShoppingAssistantService _shoppingAssistantService;
    private readonly ConsoleView _view;

    public CustomerConsoleFlow(
        InMemoryStore store,
        IStorePersistence persistence,
        CatalogService catalogService,
        CartService cartService,
        OrderService orderService,
        ReviewService reviewService,
        RecommendationService recommendationService,
        IShoppingAssistantService shoppingAssistantService,
        ConsoleView view)
    {
        _store = store;
        _persistence = persistence;
        _catalogService = catalogService;
        _cartService = cartService;
        _orderService = orderService;
        _reviewService = reviewService;
        _recommendationService = recommendationService;
        _shoppingAssistantService = shoppingAssistantService;
        _view = view;
    }

    public void Run(Customer customer)
    {
        AccessGuard.EnsureCustomer(customer);

        while (true)
        {
            var selection = _view.ShowMenu(ApplicationMenus.CreateCustomerMenu(customer.FullName));
            if (!HandleSelection(customer, selection))
            {
                return;
            }
        }
    }

    private bool HandleSelection(Customer customer, CustomerMenuSelection selection)
    {
        switch (selection)
        {
            case CustomerMenuSelection.BrowseProducts:
                _view.ShowProductsScreen("=== Product Catalog ===", _catalogService.GetAvailableProducts());
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
                ConsoleView.PrintInfo($"Wallet Balance: {customer.WalletBalance:C2}");
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
            case CustomerMenuSelection.RecommendedForYou:
                ShowRecommendations(customer);
                return true;
            case CustomerMenuSelection.AskShoppingAssistant:
                AskAssistant(customer);
                return true;
            case CustomerMenuSelection.Logout:
                return false;
            default:
                throw new ArgumentOutOfRangeException(nameof(selection), selection, null);
        }
    }

    private void SearchProducts()
    {
        Console.Clear();
        var keyword = Input.ReadRequired("Search keyword: ");
        var products = _catalogService.SearchProducts(keyword);
        Console.WriteLine("=== Search Results ===");
        _view.PrintProductList(products);
        ConsoleView.Pause();
    }

    private void AddToCart(Customer customer)
    {
        Console.Clear();
        var products = _catalogService.GetAvailableProducts();
        Console.WriteLine("=== Add Product to Cart ===");
        _view.PrintProductList(products);
        if (!products.Any())
        {
            ConsoleView.Pause();
            return;
        }

        var selected = ConsoleView.SelectProduct(products, "Select product number: ");
        var quantity = Input.ReadInt("Quantity: ", 1, 1000);

        try
        {
            _cartService.AddToCart(customer.Id, selected.Id, quantity);
            ConsoleView.PrintSuccess("Product added to cart.");
        }
        catch (DomainException ex)
        {
            ConsoleView.PrintError(ex.Message);
        }
    }

    private void ViewCart(Customer customer)
    {
        Console.Clear();
        Console.WriteLine("=== Cart ===");
        _view.PrintCartDetails(_cartService.GetCartDetails(customer.Id));
        ConsoleView.Pause();
    }

    private void UpdateCart(Customer customer)
    {
        Console.Clear();
        Console.WriteLine("=== Update Cart ===");
        var details = _cartService.GetCartDetails(customer.Id);
        if (!details.Items.Any())
        {
            Console.WriteLine("Cart is empty.");
            ConsoleView.Pause();
            return;
        }

        _view.PrintCartDetails(details);
        var selectedIndex = Input.ReadInt("Select item number: ", 1, details.Items.Count) - 1;
        var selected = details.Items[selectedIndex];
        var quantity = Input.ReadInt("New quantity (0 removes): ", 0, 1000);

        try
        {
            _cartService.UpdateQuantity(customer.Id, selected.ProductId, quantity);
            ConsoleView.PrintSuccess("Cart updated.");
        }
        catch (DomainException ex)
        {
            ConsoleView.PrintError(ex.Message);
        }
    }

    private void Checkout(Customer customer)
    {
        Console.Clear();
        try
        {
            var order = _orderService.Checkout(customer.Id);
            ConsoleView.PrintSuccess($"Checkout successful. Total: {order.TotalAmount:C2}");
        }
        catch (DomainException ex)
        {
            ConsoleView.PrintError(ex.Message);
        }
    }

    private void AddFunds(Customer customer)
    {
        Console.Clear();
        var amount = Input.ReadDecimal("Amount to add: ", 1m, 1_000_000m);
        customer.WalletBalance += amount;
        _persistence.Save(_store);
        ConsoleView.PrintSuccess($"Funds added. New balance: {customer.WalletBalance:C2}");
    }

    private void ViewOrderHistory(Customer customer)
    {
        Console.Clear();
        Console.WriteLine("=== Order History ===");
        _view.PrintOrderList(_orderService.GetCustomerOrders(customer.Id));
        ConsoleView.Pause();
    }

    private void TrackOrders(Customer customer)
    {
        Console.Clear();
        Console.WriteLine("=== Track Orders ===");
        var orders = _orderService.GetCustomerOrders(customer.Id);
        _view.PrintOrderList(orders);
        if (!orders.Any())
        {
            ConsoleView.Pause();
            return;
        }

        var selectedOrder = ConsoleView.SelectOrder(orders, "Select order number: ");
        ConsoleView.PrintInfo($"Order status: {selectedOrder.Status}");
    }

    private void ReviewProduct(Customer customer)
    {
        Console.Clear();
        var products = _catalogService.GetAvailableProducts();
        Console.WriteLine("=== Review Products ===");
        _view.PrintProductList(products);
        if (!products.Any())
        {
            ConsoleView.Pause();
            return;
        }

        var selected = ConsoleView.SelectProduct(products, "Select product number: ");
        var rating = Input.ReadInt("Rating (1-5): ", 1, 5);
        var comment = Input.ReadRequired("Comment: ");

        try
        {
            _reviewService.AddReview(customer.Id, selected.Id, rating, comment);
            ConsoleView.PrintSuccess("Review submitted.");
        }
        catch (DomainException ex)
        {
            ConsoleView.PrintError(ex.Message);
        }
    }

    private void ShowRecommendations(Customer customer)
    {
        Console.Clear();
        Console.WriteLine("=== Recommended For You ===");
        var recommendations = _recommendationService.GetRecommendations(customer.Id, 5);
        if (!recommendations.Any())
        {
            ConsoleView.PrintInfo("No recommendations available yet.");
            return;
        }

        foreach (var item in recommendations)
        {
            Console.WriteLine($"{item.Product.Name} | {item.Product.Category} | {item.Product.Price:C2} | Score: {item.Score:F2}");
            Console.WriteLine($"Reason: {string.Join(", ", item.Reasons)}");
            Console.WriteLine();
        }

        ConsoleView.Pause();
    }

    private void AskAssistant(Customer customer)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== AI Shopping Assistant ===");
            Console.WriteLine("Type your question and press Enter.");
            Console.WriteLine("Type /exit to return to the customer menu.");
            Console.WriteLine();
            Console.Write("You: ");
            var question = Console.ReadLine();

            if (string.Equals(question?.Trim(), "/exit", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(question))
            {
                continue;
            }

            var products = _catalogService.GetAvailableProducts();
            var cartDetails = _cartService.GetCartDetails(customer.Id);
            Console.Write("Assistant: ");
            _shoppingAssistantService.StreamAnswerCustomerQuestion(question, products, cartDetails, chunk => Console.Write(chunk));
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Press Enter to ask another question, or type /exit.");
            var next = Console.ReadLine();
            if (string.Equals(next?.Trim(), "/exit", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }
    }
}
