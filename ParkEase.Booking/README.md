# ParkEase.Booking — Booking Lifecycle Microservice

## Overview
Core orchestration service managing the complete parking booking lifecycle.
Runs on **port 5005**. Calls spot-service via HTTP. Uses JWT from auth-service.

---

## Project Structure

```
ParkEase.Booking/
├── BackgroundServices/
│   └── ExpiredBookingBackgroundService.cs  ← auto-cancels expired pre-bookings
├── Controllers/
│   └── BookingController.cs
├── Consumers/
│   └── BookingConsumers.cs     ← saga + driver deleted consumers
├── Data/
│   └── BookingDbContext.cs
├── DTOs/
│   └── BookingDtos.cs
├── Entities/
│   └── Booking.cs
├── Events/
│   └── BookingEvents.cs
├── Interfaces/
│   ├── IBookingRepository.cs
│   ├── IBookingService.cs
│   └── ISpotHttpClient.cs
├── Middleware/
│   └── JwtMiddleware.cs
├── Migrations/
├── Repositories/
│   └── BookingRepository.cs
├── Services/
│   ├── BookingService.cs
│   └── SpotHttpClient.cs
├── appsettings.json
├── appsettings.Development.json
└── ParkEase.Booking.csproj
```

---

## Booking Status Flow

```
Create Booking  → RESERVED
Check In        → ACTIVE
Check Out       → COMPLETED
Cancel          → CANCELLED
Auto-expire     → EXPIRED  (no check-in within 30 min grace period)
```

---

## Booking Types

| Type | Description |
|------|-------------|
| `PRE_BOOKING` | Advance reservation — requires deposit |
| `WALK_IN` | Immediate booking on arrival |

---

## Fare Calculation

```
Fare = (CheckOutTime - CheckInTime in hours) × Spot.PricePerHour
Minimum charge = 1 hour
```

---

## API Endpoints

### Driver
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/bookings` | Create booking |
| GET | `/api/v1/bookings/my-bookings` | View own bookings |
| GET | `/api/v1/bookings/{id}` | Single booking |
| PUT | `/api/v1/bookings/{id}/cancel` | Cancel booking |
| PUT | `/api/v1/bookings/{id}/checkin` | Digital check-in |
| PUT | `/api/v1/bookings/{id}/checkout` | Digital checkout + fare |
| PUT | `/api/v1/bookings/{id}/extend` | Extend duration |
| GET | `/api/v1/bookings/{id}/fare` | Preview fare |

### Manager
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/bookings/lot/{lotId}` | All bookings at lot |
| GET | `/api/v1/bookings/lot/{lotId}/active` | Active check-ins |
| PUT | `/api/v1/bookings/{id}/force-checkout` | Force checkout overstay |

### Admin
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/bookings/all` | All platform bookings |

---

## Inter-Service Communication

```
HTTP (IHttpClientFactory):
  booking-service → spot-service
    GET  /api/v1/spots/{id}       (get spot info + price)
    PUT  /api/v1/spots/{id}/reserve
    PUT  /api/v1/spots/{id}/occupy
    PUT  /api/v1/spots/{id}/release

RabbitMQ (MassTransit):
  Published:  BookingCreatedEvent, BookingCancelledEvent
              BookingCheckedInEvent, BookingCheckedOutEvent
              BookingExpiredEvent, BookingExtendedEvent
              BookingsCancelledForUserEvent (saga response)
              BookingCancellationFailedEvent (saga compensation)

  Consumed:   CancelBookingsForUserCommand (from AccountDeactivationSaga)
              DriverDeletedEvent (cascade cancel bookings)
```

---

## Background Service

```
ExpiredBookingBackgroundService
  Runs every: 5 minutes
  Action:     Auto-cancels PRE_BOOKING where
              start_time < now - 30 minutes AND status = RESERVED
  Publishes:  BookingExpiredEvent → notification-service notifies driver
```

---

## Running Locally

```bash
# Make sure spot-service is running on port 5002 first

# Start dependencies (ensure PostgreSQL and RabbitMQ are running locally)
# Run migrations
dotnet ef database update

# Start the service
dotnet run
```

Swagger UI → http://localhost:5001

---

## Testing & Quality
- **Unit Tests**: Run `dotnet test` to execute service-specific tests.
- **SonarQube**: This service is analyzed as part of the backend solution. Use the root `sonar-scan-backend.ps1` script for local analysis or check the GitHub Actions pipeline for CI results.
