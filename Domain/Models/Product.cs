namespace ConsoleShoppingApp.Domain.Models;

public sealed class Product
{
    public Product(string name, string description, string category, decimal price, int stockQuantity)
        : this(Guid.NewGuid(), name, description, category, price, stockQuantity, false)
    {
    }

    public Product(Guid id, string name, string description, string category, decimal price, int stockQuantity, bool isDeleted)
    {
        Id = id;
        Name = name;
        Description = description;
        Category = category;
        Price = price;
        StockQuantity = stockQuantity;
        IsDeleted = isDeleted;
    }

    public Guid Id { get; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsDeleted { get; set; }
}
