namespace ConsoleShoppingApp.App.Menus;

public static class ApplicationMenus
{
    public static MenuDefinition<MainMenuSelection> CreateMainMenu()
    {
        return new MenuDefinition<MainMenuSelection>(
            "=== Online Shopping Backend System ===",
            [
                new MenuOption<MainMenuSelection>(1, "Register", MainMenuSelection.Register),
                new MenuOption<MainMenuSelection>(2, "Login", MainMenuSelection.Login),
                new MenuOption<MainMenuSelection>(3, "Exit", MainMenuSelection.Exit)
            ]);
    }

    public static MenuDefinition<CustomerMenuSelection> CreateCustomerMenu(string fullName)
    {
        return new MenuDefinition<CustomerMenuSelection>(
            $"=== Customer Menu ({fullName}) ===",
            [
                new MenuOption<CustomerMenuSelection>(1, "Browse Products", CustomerMenuSelection.BrowseProducts),
                new MenuOption<CustomerMenuSelection>(2, "Search Products", CustomerMenuSelection.SearchProducts),
                new MenuOption<CustomerMenuSelection>(3, "Add Product to Cart", CustomerMenuSelection.AddProductToCart),
                new MenuOption<CustomerMenuSelection>(4, "View Cart", CustomerMenuSelection.ViewCart),
                new MenuOption<CustomerMenuSelection>(5, "Update Cart", CustomerMenuSelection.UpdateCart),
                new MenuOption<CustomerMenuSelection>(6, "Checkout", CustomerMenuSelection.Checkout),
                new MenuOption<CustomerMenuSelection>(7, "View Wallet Balance", CustomerMenuSelection.ViewWalletBalance),
                new MenuOption<CustomerMenuSelection>(8, "Add Wallet Funds", CustomerMenuSelection.AddWalletFunds),
                new MenuOption<CustomerMenuSelection>(9, "View Order History", CustomerMenuSelection.ViewOrderHistory),
                new MenuOption<CustomerMenuSelection>(10, "Track Orders", CustomerMenuSelection.TrackOrders),
                new MenuOption<CustomerMenuSelection>(11, "Review Products", CustomerMenuSelection.ReviewProducts),
                new MenuOption<CustomerMenuSelection>(12, "Logout", CustomerMenuSelection.Logout)
            ]);
    }

    public static MenuDefinition<AdminMenuSelection> CreateAdministratorMenu(string fullName)
    {
        return new MenuDefinition<AdminMenuSelection>(
            $"=== Administrator Menu ({fullName}) ===",
            [
                new MenuOption<AdminMenuSelection>(1, "Add Product", AdminMenuSelection.AddProduct),
                new MenuOption<AdminMenuSelection>(2, "Update Product", AdminMenuSelection.UpdateProduct),
                new MenuOption<AdminMenuSelection>(3, "Delete Product", AdminMenuSelection.DeleteProduct),
                new MenuOption<AdminMenuSelection>(4, "Restock Product", AdminMenuSelection.RestockProduct),
                new MenuOption<AdminMenuSelection>(5, "View Products", AdminMenuSelection.ViewProducts),
                new MenuOption<AdminMenuSelection>(6, "View Orders", AdminMenuSelection.ViewOrders),
                new MenuOption<AdminMenuSelection>(7, "Update Order Status", AdminMenuSelection.UpdateOrderStatus),
                new MenuOption<AdminMenuSelection>(8, "View Low Stock Products", AdminMenuSelection.ViewLowStockProducts),
                new MenuOption<AdminMenuSelection>(9, "Generate Sales Reports", AdminMenuSelection.GenerateSalesReports),
                new MenuOption<AdminMenuSelection>(10, "Logout", AdminMenuSelection.Logout)
            ]);
    }
}
