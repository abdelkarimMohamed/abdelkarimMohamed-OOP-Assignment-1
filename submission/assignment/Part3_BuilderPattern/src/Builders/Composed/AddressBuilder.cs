using InvoiceBuilding.Models;

namespace InvoiceBuilding.Builders.Composed;

/// <summary>
/// Task 3.3 — builds ONE complete Address. Owns every address rule, and nothing else.
/// The same class is used for billing and for shipping.
/// Mandatory: Street, City, ZipCode, Country. Optional: State.
/// </summary>
public class AddressBuilder
{
    private readonly string _label;
    private string? _street, _city, _state, _zipCode, _country;

    /// <param name="label">Used only in error messages, e.g. "Billing address".</param>
    public AddressBuilder(string label = "Address")
    {
        _label = label;
    }

    public AddressBuilder WithStreet(string street) { _street = street; return this; }
    public AddressBuilder WithCity(string city) { _city = city; return this; }
    public AddressBuilder WithState(string state) { _state = state; return this; }
    public AddressBuilder WithZipCode(string zipCode) { _zipCode = zipCode; return this; }
    public AddressBuilder WithCountry(string country) { _country = country; return this; }

    public Address Build()
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(_street)) missing.Add("Street");
        if (string.IsNullOrWhiteSpace(_city)) missing.Add("City");
        if (string.IsNullOrWhiteSpace(_zipCode)) missing.Add("ZipCode");
        if (string.IsNullOrWhiteSpace(_country)) missing.Add("Country");

        if (missing.Count > 0)
            throw new InvalidOperationException($"{_label} is incomplete. Missing: {string.Join(", ", missing)}.");

        return new Address(_street!.Trim(), _city!.Trim(),
                           string.IsNullOrWhiteSpace(_state) ? null : _state.Trim(),
                           _zipCode!.Trim(), _country!.Trim());
    }
}
