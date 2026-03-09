namespace ConsoleShoppingApp.Domain.Models;

public sealed class Customer : User
{
    public Customer(string username, string password, string fullName) : base(username, password, fullName, UserRole.Customer)
    {
        Cart = new Cart(Id);
    }

    public Customer(Guid id, string username, string password, string fullName, decimal walletBalance, Cart cart)
        : base(id, username, password, fullName, UserRole.Customer)
    {
        WalletBalance = walletBalance;
        Cart = cart;
    }

    public decimal WalletBalance { get; set; }
    public Cart Cart { get; }
}
