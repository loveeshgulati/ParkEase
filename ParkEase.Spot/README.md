# ParkEase.Spot — Parking Spot Management Microservice

## Overview
Manages individual parking spaces within lots.
Runs on **port 5004**. Uses the same JWT token issued by `ParkEase.Auth`.

---

## Project Structure

```
ParkEase.Spot/
├── Controllers/
│   └── SpotController.cs          ← all spot endpoints
├── Consumers/
│   └── SpotConsumers.cs           ← LotDeletedConsumer (cascade)
├── Data/
│   └── SpotDbContext.cs
├── DTOs/
│   └── SpotDtos.cs
├── Entities/
│   └── ParkingSpot.cs
├── Events/
│   └── SpotEvents.cs
├── Interfaces/
│   ├── ISpotRepository.cs
│   └── ISpotService.cs
├── Middleware/
│   └── JwtMiddleware.cs
├── Migrations/
├── Repositories/
│   └── SpotRepository.cs
├── Services/
│   └── SpotService.cs
├── appsettings.json
├── appsettings.Development.json
└── ParkEase.Spot.csproj
```

---

## Spot Types

| SpotType | VehicleType | Description |
|----------|-------------|-------------|
| `COMPACT` | `4W` | Small car spots |
| `STANDARD` | `4W` | Regular car spots |
| `LARGE` | `4W`, `HEAVY` | SUV / truck spots |
| `MOTORBIKE` | `2W` | Two-wheeler spots |
| `EV` | `4W`, `2W` | EV charging spots |

---

## Spot Status Transitions

```
AVAILABLE → RESERVED  (on booking creation)
RESERVED  → OCCUPIED  (on check-in)
OCCUPIED  → AVAILABLE (on checkout)
RESERVED  → AVAILABLE (on cancellation)
```

---

## API Endpoints

### Public (No Auth)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/spots/lot/{lotId}` | All spots in lot |
| GET | `/api/v1/spots/lot/{lotId}/available` | Available spots only |
| GET | `/api/v1/spots/lot/{lotId}/count` | Count available spots |
| GET | `/api/v1/spots/lot/{lotId}/type?spotType=STANDARD` | Filter by spot type |
| GET | `/api/v1/spots/lot/{lotId}/vehicle?vehicleType=4W` | Filter by vehicle type |
| GET | `/api/v1/spots/lot/{lotId}/floor/{floor}` | Filter by floor |
| GET | `/api/v1/spots/lot/{lotId}/ev` | EV charging spots |
| GET | `/api/v1/spots/lot/{lotId}/handicapped` | Handicapped spots |
| GET | `/api/v1/spots/{id}` | Single spot details |

### Manager / Admin
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/spots` | Add single spot |
| POST | `/api/v1/spots/bulk` | Bulk add spots |
| PUT | `/api/v1/spots/{id}` | Update spot |
| DELETE | `/api/v1/spots/{id}` | Delete spot |

### Internal (booking-service)
| Method | Endpoint | Description |
|--------|----------|-------------|
| PUT | `/api/v1/spots/{id}/reserve` | AVAILABLE → RESERVED |
| PUT | `/api/v1/spots/{id}/occupy` | RESERVED → OCCUPIED |
| PUT | `/api/v1/spots/{id}/release` | Any → AVAILABLE |

---

## RabbitMQ

### Published Events
| Event | Trigger |
|-------|---------|
| `SpotAddedEvent` | New spot added |
| `SpotDeletedEvent` | Spot deleted |
| `SpotStatusChangedEvent` | Status transition |
| `LotSpotCountUpdatedEvent` | After add/delete syncs lot counts |
| `SpotOccupiedEvent` | Check-in → parkinglot decrements count |
| `SpotReleasedEvent` | Checkout/cancel → parkinglot increments count |

### Consumed Events
| Event | From | Action |
|-------|------|--------|
| `LotDeletedEvent` | parkinglot-service | Cascade delete all spots in lot |

---

## Running Locally

```bash
# Start dependencies (ensure PostgreSQL and RabbitMQ are running locally)
# Run migrations
dotnet ef database update

# Start the service
dotnet run
```

Swagger UI → http://localhost:5002

---

## Testing Flow

**1. Login as manager → get token**

**2. Add single spot:**
```json
POST /api/v1/spots
Authorization: Bearer {manager_token}
{
  "lotId": 1,
  "spotNumber": "A-01",
  "floor": 0,
  "spotType": "STANDARD",
  "vehicleType": "4W",
  "pricePerHour": 30.00,
  "isHandicapped": false,
  "isEVCharging": false
}
```

**3. Bulk add spots:**
```json
POST /api/v1/spots/bulk
{
  "lotId": 1,
  "floor": 1,
  "spotType": "COMPACT",
  "vehicleType": "4W",
  "pricePerHour": 25.00,
  "count": 20,
  "prefix": "B"
}
```
Creates B-01 through B-20 automatically.

**4. Get available spots:**
```
GET /api/v1/spots/lot/1/available
```
