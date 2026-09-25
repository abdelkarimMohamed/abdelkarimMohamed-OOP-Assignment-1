using OrderSystem.Services;

namespace OrderSystem.Data;

/// <summary>Same seed data and demo scenario as the original C++ program.</summary>
public static class SampleData
{
    public static void Seed(Store store)
    {
        store.AddCustomer(1, "Mona Ali", "mona@example.com", "Cairo", isVip: true);
        store.AddCustomer(2, "Omar Hassan", "omar@example.com", "Alexandria", isVip: false);
        store.AddCustomer(3, "Sara Nabil", "sara@example.com", "Giza", isVip: false);

        store.AddProduct(101, "USB Cable", 50.00m, 100);
        store.AddProduct(102, "Wireless Mouse", 250.00m, 40);
        store.AddProduct(103, "Mechanical Keyboard", 1200.00m, 15);
        store.AddProduct(104, "Laptop Stand", 400.00m, 25);
    }

    public static void RunDemo(Store store)
    {
        store.CreateOrder(1001, 1, new DateOnly(2026, 9, 15));
        store.AddLineToOrder(1001, 101, 2);
        store.AddLineToOrder(1001, 102, 1);
        store.PayOrder(1001);

        store.CreateOrder(1002, 2, new DateOnly(2026, 9, 15));
        store.AddLineToOrder(1002, 103, 1);
        store.AddLineToOrder(1002, 104, 1);

        store.CreateOrder(1003, 3, new DateOnly(2026, 9, 16));
        store.AddLineToOrder(1003, 101, 5);
        store.PayOrder(1003);
    }
}
