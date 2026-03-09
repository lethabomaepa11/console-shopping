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
        var selectedIndex = ReadMenuSelection(menu);
        return menu.Options[selectedIndex].Selection;
    }

    private static int ReadMenuSelection<TSelection>(MenuDefinition<TSelection> menu) where TSelection : struct, Enum
    {
        var selectedIndex = 0;
        var numberBuffer = string.Empty;

        while (true)
        {
            Console.Clear();
            Console.WriteLine(menu.Title);
            Console.WriteLine("Use Up/Down arrows + Enter, or type a number.");
            Console.WriteLine();

            for (var index = 0; index < menu.Options.Count; index++)
            {
                var option = menu.Options[index];
                var marker = index == selectedIndex ? ">" : " ";
                if (index == selectedIndex)
                {
                    var previousForeground = Console.ForegroundColor;
                    var previousBackground = Console.BackgroundColor;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.WriteLine($"{marker} {option.Number}. {option.Label}");
                    Console.ForegroundColor = previousForeground;
                    Console.BackgroundColor = previousBackground;
                }
                else
                {
                    Console.WriteLine($"{marker} {option.Number}. {option.Label}");
                }
            }

            if (numberBuffer.Length > 0)
            {
                Console.WriteLine();
                Console.WriteLine($"{menu.Prompt}{numberBuffer}");
            }

            var keyInfo = Console.ReadKey(intercept: true);

            if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                selectedIndex = selectedIndex == 0 ? menu.Options.Count - 1 : selectedIndex - 1;
                numberBuffer = string.Empty;
                continue;
            }

            if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                selectedIndex = selectedIndex == menu.Options.Count - 1 ? 0 : selectedIndex + 1;
                numberBuffer = string.Empty;
                continue;
            }

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                if (numberBuffer.Length == 0)
                {
                    return selectedIndex;
                }

                if (int.TryParse(numberBuffer, out var typedSelection)
                    && typedSelection >= 1
                    && typedSelection <= menu.Options.Count)
                {
                    return typedSelection - 1;
                }

                numberBuffer = string.Empty;
                continue;
            }

            if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (numberBuffer.Length > 0)
                {
                    numberBuffer = numberBuffer[..^1];
                }

                continue;
            }

            if (char.IsDigit(keyInfo.KeyChar))
            {
                numberBuffer += keyInfo.KeyChar;
                continue;
            }
        }
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
