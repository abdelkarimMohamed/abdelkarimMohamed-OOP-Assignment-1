namespace HotelReservation.Models;

/// <summary>
/// A booking of one room by one guest for a fixed date range.
/// Its identity and dates never change; its status changes only through actions.
/// </summary>
public class Reservation
{
    /// <summary>The only legal status moves. Anything not listed here is rejected.</summary>
    private static readonly Dictionary<ReservationStatus, ReservationStatus[]> AllowedTransitions = new()
    {
        [ReservationStatus.Pending]    = new[] { ReservationStatus.Confirmed, ReservationStatus.Cancelled },
        [ReservationStatus.Confirmed]  = new[] { ReservationStatus.CheckedIn, ReservationStatus.Cancelled },
        [ReservationStatus.CheckedIn]  = new[] { ReservationStatus.CheckedOut },
        [ReservationStatus.CheckedOut] = Array.Empty<ReservationStatus>(),
        [ReservationStatus.Cancelled]  = Array.Empty<ReservationStatus>(),
    };

    public Guid ReservationId { get; }
    public DateOnly CheckInDate { get; }
    public DateOnly CheckOutDate { get; }
    public Room Room { get; }
    public Guest Guest { get; }
    public ReservationStatus Status { get; private set; }

    /// <summary>
    /// Internal: outside code cannot create a reservation directly.
    /// Reservations are created only through Guest.MakeReservation(...).
    /// All checks run before any property is set, so an invalid reservation never exists.
    /// </summary>
    internal Reservation(Guest guest, Room room, DateOnly checkInDate, DateOnly checkOutDate)
    {
        ArgumentNullException.ThrowIfNull(guest);
        ArgumentNullException.ThrowIfNull(room);
        if (checkOutDate <= checkInDate)
            throw new ArgumentException("Check-out date must be strictly after check-in date.", nameof(checkOutDate));

        room.EnsureCanBeBooked(checkInDate, checkOutDate);

        ReservationId = Guid.NewGuid();
        Guest = guest;
        Room = room;
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
        Status = ReservationStatus.Pending;
    }

    // ---------- Computed values (never set by hand) ----------

    public int Nights => CheckOutDate.DayNumber - CheckInDate.DayNumber;

    public decimal TotalCost => Nights * Room.NightlyRate;

    /// <summary>Active = still holds the room (not cancelled and not checked out).</summary>
    public bool IsActive => Status is not (ReservationStatus.Cancelled or ReservationStatus.CheckedOut);

    public bool Overlaps(DateOnly otherCheckIn, DateOnly otherCheckOut) =>
        CheckInDate < otherCheckOut && otherCheckIn < CheckOutDate;

    // ---------- Actions (the only way the status changes) ----------

    public void Confirm() => TransitionTo(ReservationStatus.Confirmed);

    public void CheckIn()
    {
        if (Status != ReservationStatus.Confirmed)
            throw new InvalidOperationException(
                $"Cannot check in: reservation must be Confirmed (current status: {Status}).");

        TransitionTo(ReservationStatus.CheckedIn);
    }

    public void CheckOut() => TransitionTo(ReservationStatus.CheckedOut);

    public void Cancel() => TransitionTo(ReservationStatus.Cancelled);

    private void TransitionTo(ReservationStatus newStatus)
    {
        if (!AllowedTransitions[Status].Contains(newStatus))
            throw new InvalidOperationException($"Illegal status change: {Status} -> {newStatus}.");

        Status = newStatus;
    }
}
