using System.Text;
using System.Text.Json;
using ConsoleShoppingApp.Domain.Models;

namespace ConsoleShoppingApp.Services;

public interface IShoppingAssistantService
{
    string AnswerCustomerQuestion(string question, IReadOnlyList<Product> products, CartDetails cartDetails);
    void StreamAnswerCustomerQuestion(string question, IReadOnlyList<Product> products, CartDetails cartDetails, Action<string> onChunk);
}

public sealed class GroqShoppingAssistantService : IShoppingAssistantService
{
    private readonly string _endpoint;
    private readonly string _model;
    private readonly HttpClient _httpClient;

    public GroqShoppingAssistantService(
        string model = "llama-3.1-8b-instant",
        string endpoint = "https://nwu-vaal-gkss.netlify.app/api/ai",
        HttpClient? httpClient = null)
    {
        _endpoint = endpoint;
        _model = model;
        _httpClient = httpClient ?? new HttpClient();
    }

    public string AnswerCustomerQuestion(string question, IReadOnlyList<Product> products, CartDetails cartDetails)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            return "Please ask a specific shopping question.";
        }

        var compactCatalog = products
            .Take(20)
            .Select(p => $"{p.Name} ({p.Category}) - {p.Price:C2}, stock: {p.StockQuantity}")
            .ToList();

        var compactCart = cartDetails.Items
            .Take(10)
            .Select(i => $"{i.ProductName} x{i.Quantity} @ {i.UnitPrice:C2}")
            .ToList();

        var systemPrompt =
            "You are a concise shopping assistant for a console e-commerce app. " +
            "Give practical buying advice using the provided catalog and cart only. " +
            "If a product is missing, say so clearly.";

        var userPrompt = new StringBuilder()
            .AppendLine("Catalog:")
            .AppendLine(compactCatalog.Any() ? string.Join("\n", compactCatalog) : "No products available.")
            .AppendLine()
            .AppendLine("Cart:")
            .AppendLine(compactCart.Any() ? string.Join("\n", compactCart) : "Cart is empty.")
            .AppendLine()
            .AppendLine("Customer question:")
            .AppendLine(question.Trim())
            .ToString();

        var mergedMessage = new StringBuilder()
            .AppendLine(systemPrompt)
            .AppendLine()
            .AppendLine(userPrompt)
            .ToString();

        var payload = new
        {
            message = mergedMessage,
            model = _model
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _endpoint);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        try
        {
            var response = _httpClient.Send(request);
            if (!response.IsSuccessStatusCode)
            {
                return $"Assistant unavailable right now (HTTP {(int)response.StatusCode}).";
            }

            var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (string.IsNullOrWhiteSpace(json))
            {
                return "Assistant returned an empty response.";
            }

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            string? answer = null;

            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("response", out var responseValue) && responseValue.ValueKind == JsonValueKind.String)
                {
                    answer = responseValue.GetString();
                }
                else if (root.TryGetProperty("message", out var messageValue) && messageValue.ValueKind == JsonValueKind.String)
                {
                    answer = messageValue.GetString();
                }
                else if (root.TryGetProperty("content", out var contentValue) && contentValue.ValueKind == JsonValueKind.String)
                {
                    answer = contentValue.GetString();
                }
            }
            else if (root.ValueKind == JsonValueKind.String)
            {
                answer = root.GetString();
            }

            return string.IsNullOrWhiteSpace(answer) ? "Assistant returned an empty response." : answer.Trim();
        }
        catch (Exception)
        {
            return "Assistant unavailable right now. Try again later.";
        }
    }

    public void StreamAnswerCustomerQuestion(string question, IReadOnlyList<Product> products, CartDetails cartDetails, Action<string> onChunk)
    {
        var answer = AnswerCustomerQuestion(question, products, cartDetails);
        StreamByWords(answer, onChunk);
    }

    private static void StreamByWords(string text, Action<string> onChunk)
    {
        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            onChunk($"{word} ");
            Thread.Sleep(15);
        }
    }
}

public sealed class LocalShoppingAssistantService : IShoppingAssistantService
{
    public string AnswerCustomerQuestion(string question, IReadOnlyList<Product> products, CartDetails cartDetails)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            return "Please ask a specific shopping question.";
        }

        var normalized = question.Trim().ToLowerInvariant();
        if (normalized.Contains("budget"))
        {
            var cheapest = products
                .Where(p => !p.IsDeleted && p.StockQuantity > 0)
                .OrderBy(p => p.Price)
                .Take(3)
                .ToList();

            if (!cheapest.Any())
            {
                return "No available products to suggest for a budget right now.";
            }

            return $"Budget picks: {string.Join(", ", cheapest.Select(p => $"{p.Name} ({p.Price:C2})"))}.";
        }

        if (normalized.Contains("cart"))
        {
            return $"Your cart has {cartDetails.Items.Count} item(s), total {cartDetails.Total:C2}.";
        }

        var topRatedOrPopular = products
            .Where(p => !p.IsDeleted && p.StockQuantity > 0)
            .OrderByDescending(p => p.StockQuantity)
            .Take(3)
            .ToList();

        if (!topRatedOrPopular.Any())
        {
            return "No product suggestions available right now.";
        }

        return $"Try: {string.Join(", ", topRatedOrPopular.Select(p => $"{p.Name} ({p.Category})"))}.";
    }

    public void StreamAnswerCustomerQuestion(string question, IReadOnlyList<Product> products, CartDetails cartDetails, Action<string> onChunk)
    {
        var answer = AnswerCustomerQuestion(question, products, cartDetails);
        foreach (var word in answer.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            onChunk($"{word} ");
            Thread.Sleep(15);
        }
    }
}
