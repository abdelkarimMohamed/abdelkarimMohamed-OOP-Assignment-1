# Critique — Procedural Order System (C++)

## Overview

`order_system.cpp` manages customers, products, orders and order lines using only
global arrays and free functions. It works for the demo scenario, but the design makes
it fragile, hard to extend, and easy to break without noticing. The problems below are
ordered roughly from the most fundamental to the most local.

---

## 1. All state is global and mutable

**Where:** lines 11–32 (`customerIds`, `productStock`, `orderIsPaid`, …).

**Why it is a problem:** every function can read and write every piece of data. Nothing
owns the data, so nothing protects it. The rules of the system (for example, "a paid
order cannot change") only hold if every function remembers to respect them.

**What could go wrong:** any new function can set `orderIsPaid[i] = false` or
`productStock[i] = -5` and the program will happily continue with corrupted data. When
a bug appears, it could have been caused by any function in the file.

## 2. One entity is split across parallel arrays

**Where:** a customer is five arrays (`customerIds`, `customerNames`, `customerEmails`,
`customerCities`, `customerIsVip`) glued together only by a shared index.

**Why it is a problem:** the concept "customer" does not exist in the code; it only exists
in the programmer's head. Keeping the arrays in sync is a manual responsibility.

**What could go wrong:** if a future change updates or removes an entry in one array and
forgets another, a customer silently gets someone else's email or VIP flag. The compiler
cannot detect this.

## 3. Hard-coded capacity limits

**Where:** `MAX_CUSTOMERS = 50`, `MAX_PRODUCTS = 50`, `MAX_ORDERS = 100`,
`MAX_LINES_PER_ORDER = 20`.

**Why it is a problem:** the limits come from the choice of fixed arrays, not from any
business rule. The two 2-D line arrays also reserve 100 × 20 slots even when only a few
lines exist.

**What could go wrong:** the 51st customer or the 21st line on an order is rejected for a
purely technical reason, and raising the limit means recompiling.

## 4. Relationships are stored as array indexes, not references

**Where:** `orderCustomerIndexes`, `lineProductIndexes`.

**Why it is a problem:** an order does not point to "its customer"; it stores a position in
an array. That position is only meaningful as long as the arrays are never reordered or
shrunk.

**What could go wrong:** the day deleting a customer or product is added, every stored index
after it points to the wrong record — orders would show the wrong customer or charge the
wrong product price, with no error.

## 5. Inconsistent API: IDs and indexes are both plain `int`

**Where:** `printOrder(int orderId)` takes an **ID**, but `calculateOrderTotal(int orderIndex)`
takes an **index**. `createOrder` returns an index; other functions expect IDs.

**Why it is a problem:** two different meanings share the same type, so the compiler cannot
tell them apart and the reader must check each function.

**What could go wrong:** passing an order ID (e.g. `1001`) to `calculateOrderTotal` reads far
outside the array — undefined behavior, garbage totals, or a crash.

## 6. Order lines do not remember the price they were sold at

**Where:** `calculateOrderTotal` and `printOrder` multiply by the *current*
`productPrices[productIndex]`.

**Why it is a problem:** an order is a historical record, but its total is recalculated from
live catalog data every time.

**What could go wrong:** if a price-change feature is ever added, the totals of already-paid
orders change retroactively, and "paid sales total" reports money that was never charged.

## 7. Business rules are scattered and hidden

**Where:** the VIP discount is `total = total * 0.90;` inside `calculateOrderTotal`.

**Why it is a problem:** the discount is a rule about the customer, but it is hidden inside an
order calculation as a magic number. The order function has to know internal details of
the customer (`customerIsVip`).

**What could go wrong:** changing the discount means searching the code for `0.90`. The
printed output is also confusing: order #1001 shows lines worth 100 + 250 = 350 but
`TOTAL: 315.00`, with no discount line to explain the difference.

## 8. Stock is consumed by orders that are never paid

**Where:** `addLineToOrder` decrements `productStock` immediately.

**Why it is a problem:** there is no way to cancel an order or remove a line, so reserved stock
can never come back.

**What could go wrong:** in the demo, unpaid order #1002 permanently holds a keyboard and a
laptop stand. Over time, abandoned orders can make products appear out of stock while
nothing was sold.

## 9. Business logic is mixed with console I/O

**Where:** almost every function calls `cout`, and `createOrder` / `addLineToOrder` print their
own errors.

**Why it is a problem:** the logic cannot be reused without a console (a web API, a GUI, a
batch import) and cannot be unit-tested without capturing text output.

**What could go wrong:** any change to how messages are shown requires touching business
functions, and it is easy to break a rule while editing a print statement.

## 10. Errors are printed, not reported

**Where:** functions print `ERROR: …` and `return` (or return `-1`).

**Why it is a problem:** the caller has no reliable way to know that the operation failed.
`runDemoScenario` ignores the result of every `createOrder` and `addLineToOrder` call.

**What could go wrong:** if `createOrder` fails, the following `addLineToOrder` calls also fail
and the program keeps going as if everything worked, leaving partial or missing data.

## 11. Missing input validation on creation

**Where:** `addCustomer`, `addProduct`, `createOrder`.

**Why it is a problem:** only duplicate IDs are checked. Nothing stops an empty name or email,
a negative price, negative stock, or a date string such as `"hello"`.

**What could go wrong:** invalid data enters the system and causes wrong totals, negative
stock counts, or meaningless reports later, far from where it was introduced.

## 12. Invariants are enforced in only one place

**Where:** "no changes after payment" is checked only in `addLineToOrder`; `markOrderPaid`
does not check whether the order is already paid.

**Why it is a problem:** because the data is global, a rule only holds where someone
remembered to write the check. There is no single guardian of an order's state.

**What could go wrong:** paying the same order twice is accepted silently; any new function that
modifies an order can bypass the paid check entirely.

## 13. `double` is used for money

**Where:** `productPrices`, `calculateOrderTotal`, `totalSalesPaidOnly`.

**Why it is a problem:** binary floating point cannot represent most decimal amounts exactly.

**What could go wrong:** small rounding errors accumulate across many orders and discounts,
producing totals that are off by a fraction and do not reconcile with accounting.

## 14. Duplicated code

**Where:** `findCustomerIndexById`, `findProductIndexById`, `findOrderIndexById` are the same
loop three times; the "order id not found" check is repeated in several functions;
`fixed << setprecision(2)` is repeated in several places.

**Why it is a problem:** the same logic must be maintained in several places.

**What could go wrong:** a fix or improvement (for example, faster lookup) is applied to one copy
and forgotten in the others, leading to inconsistent behavior.

## 15. Unsafe console input

**Where:** `cin >> choice` and the other `cin >>` reads in `runInteractiveMenu`.

**Why it is a problem:** the input is never validated. Typing a letter puts `cin` in a failed
state and sets the value to 0.

**What could go wrong:** entering `abc` at the menu makes the program print `Bye.` and exit
immediately, with no explanation and any unsaved session work lost.

---

## Summary — how an object-oriented design addresses these

| Problem | OOP answer |
|---|---|
| Global mutable state (1, 12) | Each object owns its data and is the only one allowed to change it |
| Parallel arrays, fixed sizes (2, 3) | One class per concept, stored in growable collections |
| Index-based links, ID/index confusion (4, 5) | Objects hold references to other objects; lookups by ID in one place |
| Price not snapshotted (6) | `OrderLine` stores the unit price at the time it was added |
| Hidden rules (7) | The customer knows its own discount rate; the order shows subtotal, discount and total |
| Logic mixed with I/O, silent errors (9, 10) | Domain classes throw exceptions; only the UI layer prints |
| Missing validation (11) | Constructors validate, so invalid objects cannot exist |
| `double` for money (13) | `decimal` |
| Duplication, unsafe input (14, 15) | Shared lookup logic, and input helpers that re-ask on invalid input |

Problem 8 (stock held by unpaid orders) is a business-rule gap rather than a pure design
flaw. The C# version keeps the original behavior so the features stay equivalent, but the
new design makes it easy to add cancellation later, because stock changes now go through
`Product` methods instead of direct array writes.
