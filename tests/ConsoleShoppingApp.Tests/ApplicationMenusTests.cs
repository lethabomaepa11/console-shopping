using ConsoleShoppingApp.App.Menus;

namespace ConsoleShoppingApp.Tests;

internal static class ApplicationMenusTests
{
    public static void Run()
    {
        MainMenu_ExposesExpectedOptionsInOrder();
        CustomerMenu_IncludesFullNameAndLogoutAsFinalOption();
        AdministratorMenu_UsesExpectedCommands();
    }

    private static void MainMenu_ExposesExpectedOptionsInOrder()
    {
        var menu = ApplicationMenus.CreateMainMenu();

        TestAssert.Equal("=== Online Shopping Backend System ===", menu.Title, "Main menu title mismatch.");
        TestAssert.Equal(3, menu.Options.Count, "Main menu option count mismatch.");
        TestAssert.Equal(1, menu.Options[0].Number, "Main menu register number mismatch.");
        TestAssert.Equal("Register", menu.Options[0].Label, "Main menu register label mismatch.");
        TestAssert.Equal(MainMenuSelection.Register, menu.Options[0].Selection, "Main menu register selection mismatch.");
        TestAssert.Equal(2, menu.Options[1].Number, "Main menu login number mismatch.");
        TestAssert.Equal("Login", menu.Options[1].Label, "Main menu login label mismatch.");
        TestAssert.Equal(MainMenuSelection.Login, menu.Options[1].Selection, "Main menu login selection mismatch.");
        TestAssert.Equal(3, menu.Options[2].Number, "Main menu exit number mismatch.");
        TestAssert.Equal("Exit", menu.Options[2].Label, "Main menu exit label mismatch.");
        TestAssert.Equal(MainMenuSelection.Exit, menu.Options[2].Selection, "Main menu exit selection mismatch.");
    }

    private static void CustomerMenu_IncludesFullNameAndLogoutAsFinalOption()
    {
        var menu = ApplicationMenus.CreateCustomerMenu("Jane Doe");

        TestAssert.Equal("=== Customer Menu (Jane Doe) ===", menu.Title, "Customer menu title mismatch.");
        TestAssert.Equal(12, menu.Options.Count, "Customer menu option count mismatch.");
        TestAssert.Equal(CustomerMenuSelection.BrowseProducts, menu.Options[0].Selection, "Customer first option mismatch.");
        TestAssert.Equal(CustomerMenuSelection.Logout, menu.Options[^1].Selection, "Customer logout selection mismatch.");
        TestAssert.Equal("Logout", menu.Options[^1].Label, "Customer logout label mismatch.");
        TestAssert.Equal(12, menu.Options[^1].Number, "Customer logout number mismatch.");
    }

    private static void AdministratorMenu_UsesExpectedCommands()
    {
        var menu = ApplicationMenus.CreateAdministratorMenu("System Admin");

        TestAssert.Equal("=== Administrator Menu (System Admin) ===", menu.Title, "Administrator menu title mismatch.");
        TestAssert.Equal(10, menu.Options.Count, "Administrator menu option count mismatch.");
        TestAssert.Equal(AdminMenuSelection.AddProduct, menu.Options[0].Selection, "Administrator first option mismatch.");
        TestAssert.Equal(AdminMenuSelection.GenerateSalesReports, menu.Options[8].Selection, "Administrator reports option mismatch.");
        TestAssert.Equal(AdminMenuSelection.Logout, menu.Options[9].Selection, "Administrator logout selection mismatch.");
    }
}
