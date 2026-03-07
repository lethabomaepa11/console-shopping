namespace ConsoleShoppingApp.App;

public static class Input
{
    public static int ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            var raw = Console.ReadLine();
            if (int.TryParse(raw, out var value) && value >= min && value <= max)
            {
                return value;
            }

            Console.WriteLine($"Invalid input. Enter a number between {min} and {max}.");
        }
    }

    public static decimal ReadDecimal(string prompt, decimal min, decimal max)
    {
        while (true)
        {
            Console.Write(prompt);
            var raw = Console.ReadLine();
            if (decimal.TryParse(raw, out var value) && value >= min && value <= max)
            {
                return value;
            }

            Console.WriteLine($"Invalid input. Enter a value between {min} and {max}.");
        }
    }

    public static string ReadRequired(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var value = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }

            Console.WriteLine("Input is required.");
        }
    }

    public static Guid ReadGuid(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var raw = Console.ReadLine();
            if (Guid.TryParse(raw, out var value))
            {
                return value;
            }

            Console.WriteLine("Invalid GUID format. Paste a valid product/order ID.");
        }
    }
}
