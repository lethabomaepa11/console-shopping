using ConsoleShoppingApp.Domain;

namespace ConsoleShoppingApp.Services;

public interface IPaymentStrategy
{
    Payment ProcessPayment(Customer customer, Order order);
}

public sealed class WalletPaymentStrategy : IPaymentStrategy
{
    public Payment ProcessPayment(Customer customer, Order order)
    {
        if (customer.WalletBalance < order.TotalAmount)
        {
            return new Payment(order.Id, order.TotalAmount, PaymentMethod.Wallet, PaymentStatus.Failed);
        }

        customer.WalletBalance -= order.TotalAmount;
        return new Payment(order.Id, order.TotalAmount, PaymentMethod.Wallet, PaymentStatus.Completed);
    }
}
