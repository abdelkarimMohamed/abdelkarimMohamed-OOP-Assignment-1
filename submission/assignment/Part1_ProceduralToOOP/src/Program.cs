using OrderSystem.Data;
using OrderSystem.Services;
using OrderSystem.UI;

Console.WriteLine("Object-Oriented Order System (C#)");
Console.WriteLine("Seed sample data, show a demo, then open the menu.");

var store = new Store();
SampleData.Seed(store);
SampleData.RunDemo(store);

var menu = new ConsoleMenu(store);
menu.PrintCustomers();
menu.PrintProducts();
menu.PrintAllOrders();
Console.WriteLine();
menu.PrintPaidSalesTotal();

try
{
    menu.Run();
}
catch (EndOfStreamException)
{
    Console.WriteLine("\nInput ended. Bye.");
}
