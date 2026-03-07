namespace ConsoleShoppingApp.Domain;

public abstract class User
{
    protected User(string username, string password, string fullName, UserRole role)
        : this(Guid.NewGuid(), username, password, fullName, role)
    {
    }

    protected User(Guid id, string username, string password, string fullName, UserRole role)
    {
        Id = id;
        Username = username;
        Password = password;
        FullName = fullName;
        Role = role;
    }

    public Guid Id { get; }
    public string Username { get; }
    public string Password { get; }
    public string FullName { get; }
    public UserRole Role { get; }
}
