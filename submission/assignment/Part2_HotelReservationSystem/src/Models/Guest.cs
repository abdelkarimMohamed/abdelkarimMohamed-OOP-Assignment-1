namespace HotelReservation.Models;

/// <summary>
/// A hotel guest. Identity details are fixed at registration.
/// The reservation history is visible to everyone but can only grow
/// through the guest's own action: MakeReservation.
/// </summary>
public class Guest
{
    private readonly List<Reservation> _reservations = new();

    public int GuestId { get; }
    public string FullName { get; }
    public string PhoneNumber { get; }

    public IReadOnlyList<Reservation> Reservations => _reservations.AsReadOnly();

    public Guest(int guestId, string fullName, string phoneNumber)
    {
        if (guestId <= 0)
            throw new ArgumentOutOfRangeException(nameof(guestId), "Guest id must be positive.");
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name must not be null or empty.", nameof(fullName));
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number must not be null or empty.", nameof(phoneNumber));

        GuestId = guestId;
        FullName = fullName.Trim();
        PhoneNumber = phoneNumber.Trim();
    }

    /// <summary>
    /// The only way a reservation is created and added to the guest's history.
    /// If any rule fails (dates, maintenance, double-booking) an exception is thrown
    /// and nothing is added anywhere.
    /// </summary>
    public Reservation MakeReservation(Room room, DateOnly checkInDate, DateOnly checkOutDate)
    {
        var reservation = new Reservation(this, room, checkInDate, checkOutDate);

        room.AddReservation(reservation);
        _reservations.Add(reservation);

        return reservation;
    }
}
