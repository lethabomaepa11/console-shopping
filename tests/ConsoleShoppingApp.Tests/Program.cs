namespace ConsoleShoppingApp.Tests;

internal static class Program
{
    public static int Main()
    {
        try
        {
            ApplicationMenusTests.Run();
            AccessGuardTests.Run();
            OrderObserverTests.Run();
            OrderStateTransitionTests.Run();
            Console.WriteLine("All tests passed.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }
}
