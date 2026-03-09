namespace ConsoleShoppingApp.App.Menus;

public sealed class MenuDefinition<TSelection> where TSelection : struct, Enum
{
    public MenuDefinition(string title, IReadOnlyList<MenuOption<TSelection>> options, string prompt = "Select option: ")
    {
        Title = title;
        Options = options;
        Prompt = prompt;
    }

    public string Title { get; }
    public IReadOnlyList<MenuOption<TSelection>> Options { get; }
    public string Prompt { get; }
}
