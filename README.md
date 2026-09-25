

cd D:\simulation\assignmentoop\abdelkarimMohamed-OOP-Assignment-1
@'
# abdelkarimMohamed-OOP-Assignment-1
Assignment repo for assignment/1-5 (OOP Assignment 1)

- **Name:** Karim Mohamed Ali (كريم محمد علي)
- **Student ID:** GitHub @abdelkarimMohamed
- **Track:** Backend .NET - Cycle 1
- **LeetCode:** see `submission/leetcode/account.md`

## Parts
| Part | Folder | Status |
|---|---|---|
| 1 - Procedural C++ to OOP C# | `submission/assignment/Part1_ProceduralToOOP/` | Done |
| 2 - Hotel Reservation System | `submission/assignment/Part2_HotelReservationSystem/` | Done |
| 3 - Builder Pattern | `submission/assignment/Part3_BuilderPattern/` | Done |
| 4 - LeetCode 1679 | `submission/assignment/Part4_LeetCode/` | Done |

## How to run
```bash
cd submission/assignment/Part1_ProceduralToOOP/src && dotnet run
cd submission/assignment/Part2_HotelReservationSystem/src && dotnet run
cd submission/assignment/Part3_BuilderPattern/src && dotnet run
```

## Notes & assumptions

### Part 1
- Money uses `decimal` instead of `double`.
- Order lines store the unit price at the time they are added.
- Fixed capacities (e.g. 20 lines per order) were removed; they were array-size artifacts, not business rules.
- Paying an already-paid order is now rejected (the original silently accepted it).
- The menu adds "Add customer" and "Add product" options, since the original only supported these through seed data.
- Stock is still reserved when a line is added (same as the original).

### Part 2
- `Room` prevents double-booking and booking under maintenance, because it owns its own schedule.
- Reservations are created only through `Guest.MakeReservation(...)`; the `Reservation` constructor is `internal`.
- All checks run inside the `Reservation` constructor before any property is set, so an invalid reservation never exists.
- Status transitions are defined in one table inside `Reservation`; any move not in the table throws.
- `TotalCost` uses the room's current nightly rate, as the requirement states.
- Back-to-back stays (check-out and check-in on the same day) are not an overlap.

### Part 3
- `TotalAmount` is always computed (`SubTotal - Discount + Tax`) and can never be set by hand.
- The shipping address defaults to the billing address when omitted.
- Both the single builder (3.2) and the composed builders (3.3) are kept, so they can be compared.

### Part 4
- Solved with Two Pointers (sort, then move from both ends), O(n log n) time, O(1) extra space.
'@ | Set-Content -Encoding utf8 README.md