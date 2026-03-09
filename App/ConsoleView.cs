using ConsoleShoppingApp.App.Menus;
using ConsoleShoppingApp.Domain.Models;
using ConsoleShoppingApp.Domain;
using ConsoleShoppingApp.Services;

namespace ConsoleShoppingApp.App;

public sealed class ConsoleView
{
    private readonly ReviewService _reviewService;

    public ConsoleView(ReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    public TSelection ShowMenu<TSelection>(MenuDefinition<TSelection> menu) where TSelection : struct, Enum
    {
        Console.Clear();
        Console.WriteLine(menu.Title);

        menu.Options
            .Select(option => $"{option.Number}. {option.Label}")
            .ToList()
            .ForEach(Console.WriteLine);

        var selectedIndex = Input.ReadInt(menu.Prompt, 1, menu.Options.Count) - 1;
        return menu.Options[selectedIndex].Selection;
    }

    public void PrintProductList(IReadOnlyList<Product> products)
    {
        if (!products.Any())
        {
            Console.WriteLine("No products found.");
            return;
        }

        products
            .Select((product, index) => new
            {
                index,
                product,
                averageRating = _reviewService.GetAverageRating(product.Id)
            })
            .Select(x => $"{x.index + 1}. {x.product.Name} | {x.product.Category} | {x.product.Price:C2} | Stock: {x.product.StockQuantity} | Rating: {x.averageRating:F1}")
            .ToList()
            .ForEach(Console.WriteLine);
    }

    public void ShowProductsScreen(string title, IReadOnlyList<Product> products)
    {
        Console.Clear();
        Console.WriteLine(title);
        PrintProductList(products);
        Pause();
    }

    public void PrintOrderList(IReadOnlyList<Order> orders)
    {
        if (!orders.Any())
        {
            Console.WriteLine("No orders found.");
            return;
        }

        orders
            .Select((order, index) => $"{index + 1}. {order.CreatedAt:u} | Total: {order.TotalAmount:C2} | Status: {order.Status}")
            .ToList()
            .ForEach(Console.WriteLine);
    }

    public void PrintCartDetails(CartDetails details)
    {
        if (!details.Items.Any())
        {
            Console.WriteLine("Cart is empty.");
            return;
        }

        details.Items
            .Select((item, index) => $"{index + 1}. {item.ProductName} | Qty: {item.Quantity} | Unit: {item.UnitPrice:C2} | Line: {item.LineTotal:C2}")
            .ToList()
            .ForEach(Console.WriteLine);

        Console.WriteLine($"Total: {details.Total:C2}");
    }

    public static Product SelectProduct(IReadOnlyList<Product> products, string prompt)
    {
        var index = Input.ReadInt(prompt, 1, products.Count) - 1;
        return products[index];
    }

    public static Order SelectOrder(IReadOnlyList<Order> orders, string prompt)
    {
        var index = Input.ReadInt(prompt, 1, orders.Count) - 1;
        return orders[index];
    }

    public static void PrintSuccess(string message)
    {
        Console.WriteLine(message);
        Pause();
    }

    public static void PrintError(string message)
    {
        Console.WriteLine($"Error: {message}");
        Pause();
    }

    public static void PrintInfo(string message)
    {
        Console.WriteLine(message);
        Pause();
    }

    public static void Pause()
    {
        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }
}
