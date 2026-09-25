using HotelReservation.Models;

namespace HotelReservation.Services;

/// <summary>
/// Owns all guests and rooms, and guarantees that guest IDs and room numbers are unique.
/// Business rules about rooms and reservations stay inside Room / Reservation / Guest.
/// </summary>
public class Hotel
{
    private readonly Dictionary<int, Guest> _guests = new();
    private readonly Dictionary<int, Room> _rooms = new();

    public string Name { get; }
    public IReadOnlyCollection<Guest> Guests => _guests.Values;
    public IReadOnlyCollection<Room> Rooms => _rooms.Values;

    public Hotel(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hotel name is required.", nameof(name));
        Name = name;
    }

    public Guest RegisterGuest(int guestId, string fullName, string phoneNumber)
    {
        if (_guests.ContainsKey(guestId))
            throw new InvalidOperationException($"Guest id {guestId} is already registered.");

        var guest = new Guest(guestId, fullName, phoneNumber);
        _guests.Add(guestId, guest);
        return guest;
    }

    public Room AddRoom(int roomNumber, RoomType roomType, decimal nightlyRate)
    {
        if (_rooms.ContainsKey(roomNumber))
            throw new InvalidOperationException($"Room {roomNumber} already exists.");

        var room = new Room(roomNumber, roomType, nightlyRate);
        _rooms.Add(roomNumber, room);
        return room;
    }

    public Guest GetGuest(int guestId) =>
        _guests.TryGetValue(guestId, out var guest)
            ? guest
            : throw new KeyNotFoundException($"Guest id {guestId} not found.");

    public Room GetRoom(int roomNumber) =>
        _rooms.TryGetValue(roomNumber, out var room)
            ? room
            : throw new KeyNotFoundException($"Room {roomNumber} not found.");
}
