namespace HotelReservation.Models;

public enum RoomType
{
    Single,
    Double,
    Suite
}

public enum ReservationStatus
{
    Pending,
    Confirmed,
    CheckedIn,
    CheckedOut,
    Cancelled
}
