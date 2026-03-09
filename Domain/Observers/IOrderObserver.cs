using ConsoleShoppingApp.Domain.Models;
using ConsoleShoppingApp.Domain;

namespace ConsoleShoppingApp.Domain.Observers;

public interface IOrderObserver
{
    void Update(Order order);
}
