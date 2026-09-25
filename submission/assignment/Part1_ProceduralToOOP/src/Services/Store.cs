using OrderSystem.Models;

namespace OrderSystem.Services;

/// <summary>
/// Owns every customer, product and order. This object replaces all the
/// global arrays and counters of the C++ version: state lives here, and
/// only this class decides how it is added to or looked up.
/// It contains no Console code, so it can be reused or tested without a UI.
/// </summary>
public class Store
{
    private readonly Dictionary<int, Customer> _customers = new();
    private readonly Dictionary<int, Product> _products = new();
    private readonly Dictionary<int, Order> _orders = new();

    public IReadOnlyCollection<Customer> Customers => _customers.Values;
    public IReadOnlyCollection<Product> Products => _products.Values;
    public IReadOnlyCollection<Order> Orders => _orders.Values;

    public Customer AddCustomer(int id, string name, string email, string city, bool isVip)
    {
        if (_customers.ContainsKey(id))
            throw new InvalidOperationException($"Customer id {id} already exists.");

        var customer = new Customer(id, name, email, city, isVip);
        _customers.Add(id, customer);
        return customer;
    }

    public Product AddProduct(int id, string name, decimal price, int stock)
    {
        if (_products.ContainsKey(id))
            throw new InvalidOperationException($"Product id {id} already exists.");

        var product = new Product(id, name, price, stock);
        _products.Add(id, product);
        return product;
    }

    public Order CreateOrder(int orderId, int customerId, DateOnly date)
    {
        if (_orders.ContainsKey(orderId))
            throw new InvalidOperationException($"Order id {orderId} already exists.");

        var order = new Order(orderId, GetCustomer(customerId), date);
        _orders.Add(orderId, order);
        return order;
    }

    public void AddLineToOrder(int orderId, int productId, int quantity)
        => GetOrder(orderId).AddLine(GetProduct(productId), quantity);

    public void PayOrder(int orderId) => GetOrder(orderId).MarkPaid();

    public decimal TotalPaidSales() => _orders.Values.Where(o => o.IsPaid).Sum(o => o.Total);

    public Customer GetCustomer(int id) =>
        _customers.TryGetValue(id, out var customer)
            ? customer
            : throw new KeyNotFoundException($"Customer id {id} not found.");

    public Product GetProduct(int id) =>
        _products.TryGetValue(id, out var product)
            ? product
            : throw new KeyNotFoundException($"Product id {id} not found.");

    public Order GetOrder(int id) =>
        _orders.TryGetValue(id, out var order)
            ? order
            : throw new KeyNotFoundException($"Order id {id} not found.");
}
