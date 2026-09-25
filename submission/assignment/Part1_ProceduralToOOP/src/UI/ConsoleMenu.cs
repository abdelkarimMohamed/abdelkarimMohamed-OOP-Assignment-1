using System.Globalization;
using OrderSystem.Models;
using OrderSystem.Services;

namespace OrderSystem.UI;

/// <summary>
/// All console input/output lives here. The menu talks to the Store and turns
/// any exception from the domain into a friendly "ERROR:" message, so a bad
/// action never crashes the program or leaves it in a broken state.
/// </summary>
public class ConsoleMenu
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
    private readonly Store _store;

    public ConsoleMenu(Store store)
    {
        _store = store;
    }

    public void Run()
    {
        while (true)
        {
            PrintMenu();
            int choice = ReadInt("Choice: ");
            if (choice == 0)
            {
                Console.WriteLine("Bye.");
                return;
            }

            try
            {
                Handle(choice);
            }
            catch (Exception ex) when (ex is ArgumentException
                                          or InvalidOperationException
                                          or KeyNotFoundException)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }
        }
    }

    private void Handle(int choice)
    {
        switch (choice)
        {
            case 1: PrintCustomers(); break;
            case 2: PrintProducts(); break;
            case 3: PrintAllOrders(); break;
            case 4: PrintOrder(_store.GetOrder(ReadInt("Order id: "))); break;
            case 5:
                _store.CreateOrder(ReadInt("Order id: "), ReadInt("Customer id: "), ReadDate("Date (YYYY-MM-DD): "));
                Console.WriteLine("Order created.");
                break;
            case 6:
                _store.AddLineToOrder(ReadInt("Order id: "), ReadInt("Product id: "), ReadInt("Quantity: "));
                Console.WriteLine("Line added.");
                break;
            case 7:
                _store.PayOrder(ReadInt("Order id: "));
                Console.WriteLine("Order marked as paid.");
                break;
            case 8:
                PrintPaidSalesTotal();
                break;
            case 9:
                _store.AddCustomer(ReadInt("Customer id: "), ReadText("Name: "), ReadText("Email: "),
                                   ReadText("City: "), ReadYesNo("VIP? (y/n): "));
                Console.WriteLine("Customer added.");
                break;
            case 10:
                _store.AddProduct(ReadInt("Product id: "), ReadText("Name: "),
                                  ReadDecimal("Price: "), ReadInt("Stock: "));
                Console.WriteLine("Product added.");
                break;
            default:
                Console.WriteLine("Unknown choice.");
                break;
        }
    }

    private static void PrintMenu()
    {
        Console.WriteLine();
        Console.WriteLine("---------- MENU ----------");
        Console.WriteLine("1) Print customers");
        Console.WriteLine("2) Print products");
        Console.WriteLine("3) Print all orders");
        Console.WriteLine("4) Print one order by id");
        Console.WriteLine("5) Create order");
        Console.WriteLine("6) Add line to order");
        Console.WriteLine("7) Mark order paid");
        Console.WriteLine("8) Show paid sales total");
        Console.WriteLine("9) Add customer");
        Console.WriteLine("10) Add product");
        Console.WriteLine("0) Exit");
    }

    // ---------- Output ----------

    public void PrintCustomers()
    {
        Console.WriteLine($"\n=== CUSTOMERS ({_store.Customers.Count}) ===");
        foreach (var c in _store.Customers)
            Console.WriteLine($"#{c.Id}  {c.Name}  <{c.Email}>  {c.City}  vip={(c.IsVip ? "yes" : "no")}");
    }

    public void PrintProducts()
    {
        Console.WriteLine($"\n=== PRODUCTS ({_store.Products.Count}) ===");
        foreach (var p in _store.Products)
            Console.WriteLine($"#{p.Id}  {p.Name}  price={Money(p.Price)}  stock={p.Stock}");
    }

    public void PrintAllOrders()
    {
        Console.WriteLine($"\n=== ALL ORDERS ({_store.Orders.Count}) ===");
        foreach (var order in _store.Orders)
            PrintOrder(order);
    }

    public void PrintPaidSalesTotal()
    {
        Console.WriteLine($"Paid sales total: {Money(_store.TotalPaidSales())}");
    }

    private static void PrintOrder(Order order)
    {
        Console.WriteLine($"\n=== ORDER #{order.Id} ===");
        Console.WriteLine($"Date: {order.Date:yyyy-MM-dd}");
        Console.WriteLine($"Customer: {order.Customer.Name} (#{order.Customer.Id})");
        Console.WriteLine($"Paid: {(order.IsPaid ? "yes" : "no")}");
        Console.WriteLine("Lines:");
        foreach (var line in order.Lines)
            Console.WriteLine($"  - {line.Product.Name}  x{line.Quantity}  @{Money(line.UnitPrice)}  = {Money(line.LineTotal)}");

        // Unlike the original, the discount is shown so the total is explainable.
        if (order.Discount > 0)
        {
            Console.WriteLine($"Subtotal: {Money(order.Subtotal)}");
            Console.WriteLine($"VIP discount: -{Money(order.Discount)}");
        }
        Console.WriteLine($"TOTAL: {Money(order.Total)}");
    }

    private static string Money(decimal value) => value.ToString("F2", Invariant);

    // ---------- Safe input (re-asks instead of silently exiting on bad input) ----------

    private static string ReadText(string prompt)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();
        if (input is null)
            throw new EndOfStreamException("Input ended.");
        return input.Trim();
    }

    private static int ReadInt(string prompt)
    {
        while (true)
        {
            if (int.TryParse(ReadText(prompt), out int value))
                return value;
            Console.WriteLine("Please enter a whole number.");
        }
    }

    private static decimal ReadDecimal(string prompt)
    {
        while (true)
        {
            if (decimal.TryParse(ReadText(prompt), NumberStyles.Number, Invariant, out decimal value))
                return value;
            Console.WriteLine("Please enter a number (e.g. 99.50).");
        }
    }

    private static DateOnly ReadDate(string prompt)
    {
        while (true)
        {
            if (DateOnly.TryParseExact(ReadText(prompt), "yyyy-MM-dd", Invariant, DateTimeStyles.None, out var date))
                return date;
            Console.WriteLine("Please use the format YYYY-MM-DD.");
        }
    }

    private static bool ReadYesNo(string prompt)
    {
        while (true)
        {
            string answer = ReadText(prompt).ToLowerInvariant();
            if (answer is "y" or "yes") return true;
            if (answer is "n" or "no") return false;
            Console.WriteLine("Please answer y or n.");
        }
    }
}
