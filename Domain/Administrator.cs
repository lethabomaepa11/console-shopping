namespace ConsoleShoppingApp.Domain;

public sealed class Administrator : User
{
    public Administrator(string username, string password, string fullName) : base(username, password, fullName, UserRole.Administrator)
    {
    }

    public Administrator(Guid id, string username, string password, string fullName)
        : base(id, username, password, fullName, UserRole.Administrator)
    {
    }
}
