# ParkEase.ParkingLot — Parking Lot Management Microservice

## Overview
Manages parking lot registration, approval, discovery, and availability.
Runs on **port 5003**. Uses the same JWT token issued by `ParkEase.Auth`.

---

## Project Structure

```
ParkEase.ParkingLot/
├── Controllers/
│   └── ParkingLotController.cs     ← all lot endpoints
├── Consumers/
│   ├── ManagerDeletedConsumer.cs
│   ├── LotSpotCountUpdatedConsumer.cs
│   ├── SpotOccupiedConsumer.cs
│   └── SpotReleasedConsumer.cs
├── Data/
│   └── ParkingLotDbContext.cs
├── DTOs/
│   ├── CreateLotDto.cs
│   ├── UpdateLotDto.cs
│   ├── RejectLotDto.cs
│   ├── NearbyLotsRequestDto.cs
│   ├── LotDto.cs
│   ├── NearbyLotDto.cs
│   └── ApiResponse.cs
├── Entities/
│   └── ParkingLot.cs
├── Events/
│   ├── LotCreatedEvent.cs
│   ├── LotApprovedEvent.cs
│   ├── LotRejectedEvent.cs
│   ├── LotStatusChangedEvent.cs
│   ├── LotDeletedEvent.cs
│   ├── ManagerDeletedEvent.cs
│   ├── LotSpotCountUpdatedEvent.cs
│   ├── SpotOccupiedEvent.cs
│   └── SpotReleasedEvent.cs
├── Helpers/
│   └── HaversineHelper.cs          ← GPS distance calculation
├── Interfaces/
│   ├── IParkingLotRepository.cs
│   └── IParkingLotService.cs
├── Middleware/
│   └── JwtMiddleware.cs
├── Migrations/
├── Repositories/
│   └── ParkingLotRepository.cs
├── Sagas/
│   ├── LotApprovalSagaState.cs
│   ├── SendLotApprovalNotificationCommand.cs
│   ├── LotApprovalNotificationSentEvent.cs
│   └── LotApprovalSaga.cs          ← notifies manager on approve/reject
├── Services/
│   └── ParkingLotService.cs
├── appsettings.json
├── appsettings.Development.json
├── docker-compose.yml
├── Dockerfile
└── ParkEase.ParkingLot.csproj
```

---

## API Endpoints

### Public (No Auth)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/lots/search?city=Delhi` | Search lots by city |
| GET | `/api/v1/lots/nearby?lat=X&lng=Y&radius=5` | Nearby lots via GPS |
| GET | `/api/v1/lots/{id}` | Get lot details |

### Manager
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/lots` | Register new lot (→ PENDING) |
| GET | `/api/v1/lots/my-lots` | Get own lots |
| PUT | `/api/v1/lots/{id}` | Update own lot |
| DELETE | `/api/v1/lots/{id}` | Delete own lot |
| PUT | `/api/v1/lots/{id}/toggle` | Open/close lot |

### Admin
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/lots/all` | All lots platform-wide |
| GET | `/api/v1/lots/pending` | Pending approval requests |
| PUT | `/api/v1/lots/{id}/approve` | Approve lot |
| PUT | `/api/v1/lots/{id}/reject` | Reject lot with reason |
| GET | `/api/v1/lots/manager/{managerId}` | Lots by manager |

---

## Lot Approval Flow

```
Manager creates lot
      ↓
ApprovalStatus = PENDING_APPROVAL
      ↓
Admin reviews in portal
      ↓
Admin APPROVES → APPROVED → Manager can open/add spots
Admin REJECTS  → REJECTED → Manager notified with reason
```

---

## Nearby Lots — Haversine Formula

```
GET /api/v1/lots/nearby?lat=28.6139&lng=77.2090&radius=5

Returns lots within 5km sorted by distance (nearest first).
Only returns APPROVED + OPEN lots with available spots.
```

---

## RabbitMQ

### Published Events
| Event | Trigger |
|-------|---------|
| `LotCreatedEvent` | New lot registered |
| `LotApprovedEvent` | Admin approves lot → triggers LotApprovalSaga |
| `LotRejectedEvent` | Admin rejects lot → triggers LotApprovalSaga |
| `LotStatusChangedEvent` | Lot opened/closed |
| `LotDeletedEvent` | Lot deleted |

### Consumed Events
| Event | From | Action |
|-------|------|--------|
| `ManagerDeletedEvent` | auth-service | Cascade delete all manager lots |
| `LotSpotCountUpdatedEvent` | spot-service | Update lot total/available spots |
| `SpotOccupiedEvent` | booking-service | Decrement available spots |
| `SpotReleasedEvent` | booking-service | Increment available spots |

### Saga
| Saga | Trigger | Steps |
|------|---------|-------|
| `LotApprovalSaga` | LotApprovedEvent / LotRejectedEvent | Approve/Reject → Notify manager |

---

## Running Locally

```bash
# Start dependencies
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
docker run -d --name postgres -p 5432:5432 -e POSTGRES_PASSWORD=yourpassword -e POSTGRES_DB=parkease_parkinglot postgres:16

# Run migrations
dotnet ef migrations add InitialCreate --output-dir Migrations
dotnet ef database update

# Run
dotnet run
```

Swagger UI → http://localhost:5003

---

## Testing Flow

**1. Login as manager → get token**
**2. Create a lot:**
```json
POST /api/v1/lots
Authorization: Bearer {manager_token}
{
  "name": "City Center Parking",
  "address": "Connaught Place",
  "city": "Delhi",
  "latitude": 28.6315,
  "longitude": 77.2167,
  "openTime": "08:00",
  "closeTime": "22:00"
}
```
**3. Login as admin → approve lot:**
```
PUT /api/v1/lots/{id}/approve
Authorization: Bearer {admin_token}
```
**4. Search nearby lots:**
```
GET /api/v1/lots/nearby?lat=28.6139&lng=77.2090&radius=5
```
