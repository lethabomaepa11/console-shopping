namespace ConsoleShoppingApp.App.Menus;

public sealed record MenuOption<TSelection>(int Number, string Label, TSelection Selection) where TSelection : struct, Enum;
