namespace ConsoleShoppingApp.Tests;

internal static class TestAssert
{
    public static void Equal<T>(T expected, T actual, string message)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"{message} Expected: {expected}. Actual: {actual}.");
        }
    }

    public static void True(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    public static void Throws<TException>(Action action, string expectedMessage) where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException exception)
        {
            Equal(expectedMessage, exception.Message, "Unexpected exception message.");
            return;
        }

        throw new InvalidOperationException($"Expected exception of type {typeof(TException).Name} was not thrown.");
    }
}
