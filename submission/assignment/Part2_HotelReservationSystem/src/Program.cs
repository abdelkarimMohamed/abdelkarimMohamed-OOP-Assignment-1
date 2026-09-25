using HotelReservation.Models;
using HotelReservation.Services;

// A scripted demo that exercises every requirement and business rule.
// Each "Expect failure" step must print a clear error; each "Expect success" step must pass.

var hotel = new Hotel("Nile View Hotel");

var room101 = hotel.AddRoom(101, RoomType.Single, 800m);
var room201 = hotel.AddRoom(201, RoomType.Double, 1200m);
var room301 = hotel.AddRoom(301, RoomType.Suite, 2500m);

var mona = hotel.RegisterGuest(1, "Mona Ali", "01000000001");
var omar = hotel.RegisterGuest(2, "Omar Hassan", "01000000002");

var sep26 = new DateOnly(2026, 9, 26);
var sep29 = new DateOnly(2026, 9, 29);
var oct01 = new DateOnly(2026, 10, 1);

Section("1. Happy path: book -> confirm -> check in -> check out");
Reservation monaStay = null!;
ExpectSuccess("Mona books room 101 for 3 nights", () =>
{
    monaStay = mona.MakeReservation(room101, sep26, sep29);
    Console.WriteLine($"     {Describe(monaStay)}");
});
ExpectSuccess("Confirm", () => monaStay.Confirm());
ExpectSuccess("Check in", () => monaStay.CheckIn());
ExpectSuccess("Check out", () => monaStay.CheckOut());
Console.WriteLine($"     Final status: {monaStay.Status}");

Section("2. Date validity");
ExpectFailure("Check-out before check-in", () => omar.MakeReservation(room201, sep29, sep26));
ExpectFailure("Check-out on the same day as check-in", () => omar.MakeReservation(room201, sep26, sep26));

Section("3. No booking under maintenance");
ExpectSuccess("Start maintenance on room 301", () => room301.StartMaintenance());
ExpectFailure("Book room 301 while under maintenance", () => omar.MakeReservation(room301, sep26, sep29));
ExpectSuccess("End maintenance on room 301", () => room301.EndMaintenance());
ExpectSuccess("Book room 301 after maintenance", () => omar.MakeReservation(room301, sep26, sep29));

Section("4. Legal status transitions only");
var omarStay = omar.MakeReservation(room201, sep26, sep29);
ExpectFailure("Check in a Pending reservation (not confirmed)", () => omarStay.CheckIn());
ExpectFailure("Check out a Pending reservation", () => omarStay.CheckOut());
ExpectFailure("Check in again after checking out", () => monaStay.CheckIn());
ExpectFailure("Cancel an already checked-out reservation", () => monaStay.Cancel());
ExpectSuccess("Cancel a Pending reservation", () => omarStay.Cancel());
ExpectFailure("Confirm a Cancelled reservation", () => omarStay.Confirm());

Section("5. Positive pricing");
ExpectFailure("Set room 201 rate to 0", () => room201.UpdateNightlyRate(0m));
ExpectFailure("Set room 201 rate to -100", () => room201.UpdateNightlyRate(-100m));
ExpectFailure("Create a room with a negative rate", () => hotel.AddRoom(401, RoomType.Single, -1m));
ExpectSuccess("Set room 201 rate to 1500", () => room201.UpdateNightlyRate(1500m));

Section("6. Non-empty identity fields");
ExpectFailure("Register a guest with an empty name", () => hotel.RegisterGuest(3, "", "01000000003"));
ExpectFailure("Register a guest with a blank phone", () => hotel.RegisterGuest(3, "Sara Nabil", "   "));
ExpectFailure("Register a duplicate guest id", () => hotel.RegisterGuest(1, "Someone Else", "01000000009"));

Section("7. No double-booking");
Reservation first = null!;
ExpectSuccess("Mona books room 201 from Sep 26 to Oct 1", () => first = mona.MakeReservation(room201, sep26, oct01));
ExpectFailure("Omar books room 201 from Sep 29 to Oct 1 (overlaps)", () => omar.MakeReservation(room201, sep29, oct01));
ExpectSuccess("Omar books room 201 from Oct 1 (back-to-back, no overlap)", () =>
    omar.MakeReservation(room201, oct01, oct01.AddDays(2)));
ExpectSuccess("Mona cancels her booking", () => first.Cancel());
ExpectSuccess("Omar can now book Sep 29 to Oct 1", () => omar.MakeReservation(room201, sep29, oct01));

Section("8. Encapsulation of the reservation history");
Console.WriteLine($"     Mona has {mona.Reservations.Count} reservations, Omar has {omar.Reservations.Count}.");
ExpectFailure("Cast Guest.Reservations back to a List and add to it", () =>
{
    var asList = (IList<Reservation>)mona.Reservations;
    asList.Add(monaStay);
});
Console.WriteLine($"     Mona still has {mona.Reservations.Count} reservations.");

Section("9. Total cost follows the room's nightly rate");
var costCheck = mona.MakeReservation(room301, oct01, oct01.AddDays(4));
Console.WriteLine($"     {Describe(costCheck)}");
room301.UpdateNightlyRate(3000m);
Console.WriteLine($"     After price change -> {Describe(costCheck)}");

Console.WriteLine();
Console.WriteLine("Demo finished.");

// ---------- helpers ----------

static void Section(string title)
{
    Console.WriteLine();
    Console.WriteLine($"=== {title} ===");
}

static void ExpectSuccess(string description, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  [OK]      {description}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  [UNEXPECTED ERROR] {description}: {ex.Message}");
    }
}

static void ExpectFailure(string description, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  [NOT BLOCKED!] {description}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  [BLOCKED] {description}");
        Console.WriteLine($"            -> {ex.GetType().Name}: {ex.Message}");
    }
}

static string Describe(Reservation r) =>
    $"Reservation {r.ReservationId.ToString()[..8]} | {r.Guest.FullName} | Room {r.Room.RoomNumber} ({r.Room.RoomType}) | " +
    $"{r.CheckInDate:yyyy-MM-dd} -> {r.CheckOutDate:yyyy-MM-dd} | {r.Nights} nights x {r.Room.NightlyRate:F2} = {r.TotalCost:F2} | {r.Status}";
