# Carpool.Tests — test suite report

Automated tests for the Carpool Web API. Covers the Stage 14 requirements of the project
spec: service-layer business-rule unit tests with mocked repositories (§70) and the
two-`DbContext` optimistic-concurrency test (§69).

**60 tests total — 58 unit + 2 integration. All passing.**

```
BookingServiceTests ....... 17
RideServiceTests .......... 18
RatingServiceTests ........ 11
VehicleServiceTests ...... 12
RideSeatConcurrencyTests .. 2   (integration — real PostgreSQL)
```

---

## 1. Running the tests

```bash
# everything (needs a local PostgreSQL for the 2 integration tests)
dotnet test Carpool.Tests

# unit tests only — no database required
dotnet test Carpool.Tests --filter "Category!=Integration"
```

The 2 integration tests are marked `[Trait("Category", "Integration")]` and
`[SkippableFact]`. If PostgreSQL is not reachable they **skip** (they do not fail), so the
suite is green on any machine; when a database is available they run for real.

Integration-test database: taken from the `CARPOOL_TEST_DB` environment variable, otherwise
`Host=localhost;Port=5432;Database=carpool_test;Username=postgres;Password=postgres`. It is a
**throwaway** database — created and migrated before the tests, dropped afterwards. The
development `carpool` database is never touched.

---

## 2. Test mechanism

### 2.1 Unit tests (58) — mocks, no HTTP, no database

These do **not** use an API client (`HttpClient` / `WebApplicationFactory`) and do **not**
touch a database or EF Core. Each test constructs the real service class directly and hands
it fake collaborators:

| Collaborator | How it is supplied |
|---|---|
| Repositories (`IUserRepository`, `IRideRepository`, …) | **Moq** mocks. `.Setup(...)` controls what "the database" returns; `.Verify(...)` asserts what the service tried to persist. |
| `IUnitOfWork` | A Moq mock from `TestMocks.UnitOfWork()`. `SaveChangesAsync` returns `1`; **`ExecuteInTransactionAsync` is set up to actually invoke the delegate passed to it**, so the transactional body of a service method (seat decrement + booking insert, status change + booking cascade, …) really executes under test. |
| `IMapper` | The **real** AutoMapper, built by `TestMapper.Create()` from the production profiles (`cfg.AddMaps(typeof(TagProfile).Assembly)` — the same call `Program.cs` makes). The tests therefore also exercise the entity → DTO mapping. |
| Entities | `TestData` static builders — e.g. `TestData.Ride(id, driverId, status: RideStatus.Scheduled, availableSeats: 1)` — with sensible defaults, overridable per test. |

Why mock the repositories: spec §70 — *"Focus Service-layer tests on business logic.
Repositories should be mocked."* The point of these tests is the decision logic (ownership
checks, seat math, state-transition guards, rating eligibility), not SQL.

Typical shape:

```csharp
var ride = TestData.Ride(RideId, Driver, availableSeats: 1);
_rides.Setup(r => r.GetByIdAsync(RideId, It.IsAny<CancellationToken>())).ReturnsAsync(ride);
_bookings.Setup(b => b.HasActiveBookingAsync(RideId, Passenger, It.IsAny<CancellationToken>()))
         .ReturnsAsync(false);

await Assert.ThrowsAsync<BusinessRuleException>(() =>
    CreateSut().CreateAsync(RideId, Passenger, new CreateBookingRequest { NumberOfSeats = 2 }, default));

Assert.Equal(1, ride.AvailableSeats);                                   // unchanged
_bookings.Verify(b => b.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Never);
_unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
```

Assertions come in three flavours:
- **Exception type** — `Assert.ThrowsAsync<NotFoundException>` / `BusinessRuleException` /
  `ForbiddenException` / `ConflictException`. These map 1:1 to HTTP 404 / 400 / 403 / 409
  via the API's exception middleware (§71).
- **Entity state** — the in-memory entity was mutated correctly (`ride.AvailableSeats == 2`,
  `booking.Status == Cancelled`, `booking.CancelledAt` set).
- **Interaction** — `Mock.Verify(...)` that a repository / unit-of-work method was called
  exactly once, or never (e.g. nothing is persisted when validation fails).

### 2.2 Integration tests (2) — real PostgreSQL, two `DbContext`s, no mocks

The optimistic-concurrency guarantee relies on PostgreSQL's `xmin` system column, which the
EF InMemory and SQLite providers do not have — so this test needs a real database.

`PostgresTestDatabase` is an xUnit `IAsyncLifetime` **collection fixture**:
- `InitializeAsync` — `EnsureDeletedAsync()` then `MigrateAsync()` on `carpool_test`
  (migrations also apply the `HasData` seed, so seeded ride **#1** exists). On any connection
  failure it records `Available = false` instead of throwing.
- `NewContext()` — hands out a fresh `CarpoolDbContext` bound to that database.
- `DisposeAsync` — drops `carpool_test`.

No repositories, no services (except `CarpoolUnitOfWork` in the second test) — the test drives
`DbContext` directly, exactly as spec §69 describes.

---

## 3. Coverage vs. spec §70 minimum list

| §70 requirement | Test |
|---|---|
| Successful booking | `BookingServiceTests.CreateAsync_WithAvailableSeats_DecrementsSeatsAndInsertsBooking` |
| Booking rejection when insufficient seats | `BookingServiceTests.CreateAsync_WhenNotEnoughSeats_ThrowsBusinessRuleAndDoesNotInsert` |
| Driver cannot book own ride | `BookingServiceTests.CreateAsync_WhenBookerIsTheDriver_ThrowsBusinessRule` |
| Duplicate active booking | `BookingServiceTests.CreateAsync_WhenPassengerAlreadyHasActiveBooking_ThrowsConflict` |
| Booking cancellation | `BookingServiceTests.CancelAsync_WhenActiveAndRideScheduled_CancelsAndReturnsSeats` |
| Ride cancellation | `RideServiceTests.CancelAsync_WhenScheduledAndCallerIsDriver_CancelsRideAndActiveBookings` |
| Ride completion | `RideServiceTests.CompleteAsync_WhenInProgress_CompletesRideAndActiveBookings` |
| Rating eligibility | `RatingServiceTests` (7 cases — see §4.3) |
| Limited-resource concurrency scenario | `RideSeatConcurrencyTests` (§69) |

---

## 4. Full test-case catalogue

### 4.1 `BookingServiceTests` (17)

Collaborators: `Mock<IBookingRepository>`, `Mock<IRideRepository>`, mocked `IUnitOfWork`, real `IMapper`.

**`CreateAsync`**

| Test | Scenario → expected |
|---|---|
| `CreateAsync_WithAvailableSeats_DecrementsSeatsAndInsertsBooking` | Scheduled ride, 4 seats free, book 2 → `AvailableSeats` becomes 2; `AddAsync` called once with `Status = Active`, correct ride/passenger/seat count; `SaveChangesAsync` once; response `NumberOfSeats == 2`. |
| `CreateAsync_WhenNotEnoughSeats_ThrowsBusinessRuleAndDoesNotInsert` | 1 seat free, request 2 → `BusinessRuleException`; seats unchanged; nothing added or saved. |
| `CreateAsync_WhenBookerIsTheDriver_ThrowsBusinessRule` | Caller id == `ride.DriverId` → `BusinessRuleException`; nothing added. |
| `CreateAsync_WhenPassengerAlreadyHasActiveBooking_ThrowsConflict` | `HasActiveBookingAsync` returns true → `ConflictException` (HTTP 409); nothing added. |
| `CreateAsync_WhenRideDoesNotExist_ThrowsNotFound` | `GetByIdAsync` returns null → `NotFoundException`. |
| `CreateAsync_WhenRideIsNotScheduled_ThrowsBusinessRule` | **Theory** — ride status `InProgress` / `Completed` / `Cancelled` → `BusinessRuleException`. |
| `CreateAsync_WhenRideAlreadyDeparted_ThrowsBusinessRule` | Status still Scheduled but `DepartureTime` in the past → `BusinessRuleException`. |
| `CreateAsync_WhenNumberOfSeatsNotPositive_ThrowsBusinessRule` | `NumberOfSeats == 0` → `BusinessRuleException`. |

**`CancelAsync`**

| Test | Scenario → expected |
|---|---|
| `CancelAsync_WhenActiveAndRideScheduled_CancelsAndReturnsSeats` | Owner cancels an Active booking of 2 seats on a Scheduled ride → `booking.Status = Cancelled`, `CancelledAt` set, `ride.AvailableSeats` += 2; `SaveChangesAsync` once. |
| `CancelAsync_WhenNotTheOwner_ThrowsForbidden` | Different user id → `ForbiddenException`; nothing saved. (No Admin bypass — §25.) |
| `CancelAsync_WhenBookingNotActive_ThrowsBusinessRule` | Booking already Cancelled → `BusinessRuleException`. |
| `CancelAsync_WhenRideNoLongerScheduled_ThrowsBusinessRule` | Ride is InProgress → `BusinessRuleException`. |
| `CancelAsync_WhenBookingNotFound_ThrowsNotFound` | `GetByIdAsync` returns null → `NotFoundException`. |

**`GetByIdAsync`**

| Test | Scenario → expected |
|---|---|
| `GetByIdAsync_WhenNeitherOwnerNorAdmin_ThrowsForbidden` | Other user, role `User` → `ForbiddenException`. |
| `GetByIdAsync_AsAdmin_ReturnsBookingOfAnotherUser` | Other user, role `Admin` → returns the booking. |

### 4.2 `RideServiceTests` (18)

Collaborators: `Mock<IRideRepository>`, `Mock<IVehicleRepository>`, `Mock<ITagRepository>`, mocked `IUnitOfWork`, real `IMapper`.

**`CancelAsync`**

| Test | Scenario → expected |
|---|---|
| `CancelAsync_WhenScheduledAndCallerIsDriver_CancelsRideAndActiveBookings` | 2 Active + 1 Cancelled booking → ride `Cancelled`; both Active bookings → `Cancelled` with `CancelledAt`; the already-Cancelled one untouched; `SaveChangesAsync` once. |
| `CancelAsync_AsAdmin_CanCancelAnotherDriversRide` | Non-driver Admin → ride `Cancelled`. |
| `CancelAsync_WhenCallerIsNotDriverNorAdmin_ThrowsForbidden` | Non-driver `User` → `ForbiddenException`; nothing saved. |
| `CancelAsync_WhenRideNotScheduled_ThrowsBusinessRule` | **Theory** — `InProgress` / `Completed` / `Cancelled` → `BusinessRuleException` (§18: no cancel after InProgress). |
| `CancelAsync_WhenRideNotFound_ThrowsNotFound` | null → `NotFoundException`. |

**`CompleteAsync`**

| Test | Scenario → expected |
|---|---|
| `CompleteAsync_WhenInProgress_CompletesRideAndActiveBookings` | InProgress ride, 1 Active + 1 Cancelled booking → ride `Completed`, `CompletedAt` + `StartedAt` set; Active booking → `Completed`; Cancelled one untouched; `SaveChangesAsync` once. |
| `CompleteAsync_WhenScheduledButDeparted_IsAllowed` | Scheduled, `DepartureTime` 30 min ago (background service hasn't caught up) → allowed; ride `Completed`, `StartedAt` + `CompletedAt` set. |
| `CompleteAsync_WhenScheduledAndNotYetDeparted_ThrowsBusinessRule` | Scheduled, departure 3 h in the future → `BusinessRuleException`; nothing saved. |
| `CompleteAsync_WhenAlreadyFinished_ThrowsBusinessRule` | **Theory** — `Completed` / `Cancelled` → `BusinessRuleException`. |
| `CompleteAsync_WhenCallerIsNotDriverNorAdmin_ThrowsForbidden` | Non-driver `User` → `ForbiddenException`. |

**`CreateAsync`**

| Test | Scenario → expected |
|---|---|
| `CreateAsync_WithOwnedVehicleAndValidSeats_InitialisesRide` | Caller owns the vehicle (capacity 4), `TotalSeats = 3` → added ride has `DriverId` = caller, `AvailableSeats == TotalSeats == 3`, `Status = Scheduled`; `SaveChangesAsync` once. |
| `CreateAsync_WhenVehicleNotOwnedByCaller_ThrowsForbidden` | Vehicle owned by someone else → `ForbiddenException`; nothing added. (No Admin bypass — §12.) |
| `CreateAsync_WhenVehicleNotFound_ThrowsNotFound` | null vehicle → `NotFoundException`. |
| `CreateAsync_WhenTotalSeatsExceedVehicleCapacity_ThrowsBusinessRule` | Capacity 2, `TotalSeats = 3` → `BusinessRuleException`. |
| `CreateAsync_WhenAnyTagIdIsUnknown_ThrowsBusinessRule` | Request tag ids `[1, 99]`, repo resolves only `1` → `BusinessRuleException`; nothing added. |

### 4.3 `RatingServiceTests` (11)

Collaborators: `Mock<IRatingRepository>`, `Mock<IRideRepository>`, mocked `IUnitOfWork`, real `IMapper`.

| Test | Scenario → expected |
|---|---|
| `CreateAsync_WhenEligible_AddsRatingWithDriverCopiedFromRide` | Completed ride, reviewer has a Completed booking, not the driver, no prior rating → `AddAsync` once with `RideId` / `ReviewerId` set and **`DriverId` copied from the ride**; score preserved; `SaveChangesAsync` once. |
| `CreateAsync_WhenScoreOutOfRange_ThrowsBusinessRule` | **Theory** — score `0` and `6` → `BusinessRuleException`. |
| `CreateAsync_WhenRideNotFound_ThrowsNotFound` | null ride → `NotFoundException`. |
| `CreateAsync_WhenRideNotCompleted_ThrowsBusinessRule` | **Theory** — `Scheduled` / `InProgress` / `Cancelled` → `BusinessRuleException`. |
| `CreateAsync_WhenReviewerIsTheDriver_ThrowsBusinessRule` | Caller id == `ride.DriverId` → `BusinessRuleException` (can't rate yourself). |
| `CreateAsync_WhenReviewerHasNoCompletedBookingOnTheRide_ThrowsBusinessRule` | Reviewer's booking is Cancelled (and another user's is Completed) → `BusinessRuleException`. |
| `CreateAsync_WhenReviewerAlreadyRatedTheRide_ThrowsConflict` | `ExistsForReviewerAsync` returns true → `ConflictException`; nothing added. |
| `GetForUserAsync_ReturnsMappedRatings` | Repo returns 2 ratings for the driver → 2 mapped `RatingResponse` items. |

### 4.4 `VehicleServiceTests` (12)

Collaborators: `Mock<IVehicleRepository>`, mocked `IUnitOfWork`, real `IMapper`.

| Test | Scenario → expected |
|---|---|
| `CreateAsync_SetsOwnerFromCallerAndPersists` | Plate free → added vehicle has `OwnerId` = **caller id** (never from the request body, §43); `SaveChangesAsync` once. |
| `CreateAsync_WhenLicensePlateTaken_ThrowsConflict` | `LicensePlateExistsAsync(plate, null)` true → `ConflictException`; nothing added. |
| `GetByIdAsync_WhenNotFound_ThrowsNotFound` | null → `NotFoundException`. |
| `GetByIdAsync_WhenNeitherOwnerNorAdmin_ThrowsForbidden` | Other user, `User` → `ForbiddenException`. |
| `GetByIdAsync_AsAdmin_ReturnsAnotherUsersVehicle` | Other user, `Admin` → returns it. |
| `UpdateAsync_WhenOwner_AppliesChangesAndPersists` | Owner updates make/model/plate/capacity → entity fields updated, `OwnerId` unchanged; `Update` + `SaveChangesAsync` once. |
| `UpdateAsync_WhenNotOwnerNorAdmin_ThrowsForbidden` | Other `User` → `ForbiddenException`. |
| `UpdateAsync_WhenNewPlateBelongsToAnotherVehicle_ThrowsConflict` | `LicensePlateExistsAsync(plate, excludeVehicleId)` true → `ConflictException`; nothing saved. |
| `DeleteAsync_WhenNotReferencedByAnyRide_RemovesVehicle` | `IsReferencedByAnyRideAsync` false → `Remove` + `SaveChangesAsync` once. |
| `DeleteAsync_WhenReferencedByARide_ThrowsConflictAndDoesNotRemove` | `IsReferencedByAnyRideAsync` true → `ConflictException` (§9); nothing removed or saved. |
| `DeleteAsync_WhenNotOwnerNorAdmin_ThrowsForbidden` | Other `User` → `ForbiddenException`. |
| `GetMineAsync_ReturnsOnlyTheCallersVehicles` | Repo returns the caller's 2 vehicles → 2 mapped responses, all with the caller's `OwnerId`. |

### 4.5 `RideSeatConcurrencyTests` (2) — integration, §69

| Test | Scenario → expected |
|---|---|
| `TwoContexts_BothDecrementSameRideSeats_SecondSaveThrowsDbUpdateConcurrencyException` | Seeded ride reset to 3 seats. `contextA` and `contextB` each read the ride (both see 3). `contextA` sets seats to 1 and saves — **succeeds**. `contextB` sets seats to 1 and saves — **throws `DbUpdateConcurrencyException`** because its `xmin` is now stale. A third context confirms the row is `1`, never `-1`. |
| `CarpoolUnitOfWork_TranslatesTheConflict_ToConcurrencyConflictException` | Same race, but `contextB`'s save goes through `CarpoolUnitOfWork.SaveChangesAsync`, which catches EF's `DbUpdateConcurrencyException` and rethrows the domain `ConcurrencyConflictException` — the exact type the API middleware maps to **HTTP 409** (§23, §24). |

---

## 5. Deliberately not covered here

- **Controllers, middleware, JWT pipeline, Swagger.** Spec §70 scopes automated tests to
  the service layer. Those layers were verified with live `curl` walk-throughs during
  Stages 9–12 (auth → token → CRUD → error mapping → correlation id → Swagger doc).
- **Repository SQL / EF query translation.** Mocked away by design; a mock cannot verify a
  LINQ-to-SQL translation. The queries were exercised against the live database in the
  Stage 6 repository smoke test.
- **`RideStatusService` / the hosted background service.** Verified live in Stage 13 by
  planting past-departure rides and observing the transitions + booking cascade.
- **AutoMapper profile completeness.** Verified separately by
  `AssertConfigurationIsValid()` in the Stage 7 smoke test; the unit tests here use the
  real mapper so a broken profile would also surface as a failing service test.

---

## 6. Packages

| Package | Purpose |
|---|---|
| `Microsoft.NET.Test.Sdk` | test host |
| `xunit`, `xunit.runner.visualstudio` | test framework + runner |
| `Moq` | repository / unit-of-work mocks |
| `Xunit.SkippableFact` | conditional skip for the integration tests when no database is present |
| `Microsoft.EntityFrameworkCore`, `Npgsql.EntityFrameworkCore.PostgreSQL` | the two-`DbContext` concurrency test |
| `AutoMapper` (via `Carpool.Service`) | the real mapper used by `TestMapper` |

## 7. File layout

```
Carpool.Tests/
├── TestKit/
│   ├── TestMapper.cs            real IMapper from the production profiles
│   ├── TestMocks.cs             pre-wired IUnitOfWork mock (runs the transaction delegate)
│   ├── TestData.cs              entity builders (User / Vehicle / Ride / Booking)
│   └── PostgresTestDatabase.cs  IAsyncLifetime fixture: create/migrate/drop carpool_test
├── Services/
│   ├── BookingServiceTests.cs
│   ├── RideServiceTests.cs
│   ├── RatingServiceTests.cs
│   └── VehicleServiceTests.cs
└── Concurrency/
    └── RideSeatConcurrencyTests.cs
```
