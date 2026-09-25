namespace HotelReservation.Models;

/// <summary>
/// A physical room in the hotel.
/// The room owns its own schedule, so it is the class responsible for
/// refusing bookings while under maintenance and for preventing double-booking.
/// </summary>
public class Room
{
    private readonly List<Reservation> _reservations = new();

    public int RoomNumber { get; }
    public RoomType RoomType { get; }
    public decimal NightlyRate { get; private set; }
    public bool IsUnderMaintenance { get; private set; }

    /// <summary>Every reservation ever made for this room (read-only from outside).</summary>
    public IReadOnlyList<Reservation> Reservations => _reservations.AsReadOnly();

    public Room(int roomNumber, RoomType roomType, decimal nightlyRate)
    {
        if (roomNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(roomNumber), "Room number must be positive.");
        if (!Enum.IsDefined(roomType))
            throw new ArgumentOutOfRangeException(nameof(roomType), "Unknown room type.");

        RoomNumber = roomNumber;
        RoomType = roomType;
        UpdateNightlyRate(nightlyRate);
    }

    // ---------- Pricing ----------

    /// <summary>The only way to change the price. Rejects zero or negative values.</summary>
    public void UpdateNightlyRate(decimal newRate)
    {
        if (newRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(newRate), "Nightly rate must be a positive value.");

        NightlyRate = newRate;
    }

    // ---------- Maintenance ----------

    public void StartMaintenance()
    {
        if (IsUnderMaintenance)
            throw new InvalidOperationException($"Room {RoomNumber} is already under maintenance.");

        IsUnderMaintenance = true;
    }

    public void EndMaintenance()
    {
        if (!IsUnderMaintenance)
            throw new InvalidOperationException($"Room {RoomNumber} is not under maintenance.");

        IsUnderMaintenance = false;
    }

    // ---------- Availability ----------

    /// <summary>True if no active reservation of this room overlaps the given dates.</summary>
    public bool IsAvailable(DateOnly checkIn, DateOnly checkOut) =>
        !_reservations.Any(r => r.IsActive && r.Overlaps(checkIn, checkOut));

    /// <summary>
    /// Throws if the room cannot be booked for these dates.
    /// Called by the Reservation constructor BEFORE the reservation is fully created.
    /// </summary>
    internal void EnsureCanBeBooked(DateOnly checkIn, DateOnly checkOut)
    {
        if (IsUnderMaintenance)
            throw new InvalidOperationException($"Room {RoomNumber} is under maintenance and cannot be booked.");
        if (!IsAvailable(checkIn, checkOut))
            throw new InvalidOperationException(
                $"Room {RoomNumber} is already booked between {checkIn:yyyy-MM-dd} and {checkOut:yyyy-MM-dd}.");
    }

    /// <summary>Registers a newly created reservation on this room's schedule.</summary>
    internal void AddReservation(Reservation reservation)
    {
        ArgumentNullException.ThrowIfNull(reservation);
        if (reservation.Room != this)
            throw new InvalidOperationException("This reservation belongs to a different room.");

        EnsureCanBeBooked(reservation.CheckInDate, reservation.CheckOutDate);
        _reservations.Add(reservation);
    }
}
