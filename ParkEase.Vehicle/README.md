# ParkEase.Vehicle — Vehicle Management Microservice

## Overview
Manages all vehicles registered by drivers on the ParkEase platform.
Runs on **port 5002**. Uses the same JWT token issued by `ParkEase.Auth`.

---

## Project Structure

```
ParkEase.Vehicle/
├── Controllers/
│   └── VehicleController.cs       ← all vehicle endpoints
├── Consumers/
│   └── DriverDeletedConsumer.cs   ← cascade delete on driver removal
├── Data/
│   └── VehicleDbContext.cs
├── DTOs/
│   ├── RegisterVehicleDto.cs
│   ├── UpdateVehicleDto.cs
│   ├── VehicleDto.cs
│   └── ApiResponse.cs
├── Entities/
│   └── Vehicle.cs
├── Events/
│   ├── VehicleRegisteredEvent.cs
│   ├── VehicleUpdatedEvent.cs
│   ├── VehicleDeletedEvent.cs
│   └── DriverDeletedEvent.cs
├── Interfaces/
│   ├── IVehicleRepository.cs
│   └── IVehicleService.cs
├── Middleware/
│   └── JwtMiddleware.cs
├── Migrations/
├── Repositories/
│   └── VehicleRepository.cs
├── Services/
│   └── VehicleService.cs
├── appsettings.json
├── appsettings.Development.json
├── docker-compose.yml
├── Dockerfile
└── ParkEase.Vehicle.csproj
```

---

## API Endpoints

| Method | Endpoint | Role | Description |
|--------|----------|------|-------------|
| POST | `/api/v1/vehicles` | DRIVER | Register new vehicle |
| GET | `/api/v1/vehicles/my-vehicles` | DRIVER | Get own vehicles |
| GET | `/api/v1/vehicles/{id}` | DRIVER, ADMIN | Get vehicle by id |
| GET | `/api/v1/vehicles/owner/{ownerId}` | ADMIN | Get all vehicles by owner |
| GET | `/api/v1/vehicles/all` | ADMIN | Get all platform vehicles |
| PUT | `/api/v1/vehicles/{id}` | DRIVER | Update own vehicle |
| DELETE | `/api/v1/vehicles/{id}` | DRIVER, ADMIN | Delete vehicle |
| GET | `/api/v1/vehicles/{id}/type` | DRIVER, ADMIN | Get vehicle type |
| GET | `/api/v1/vehicles/{id}/is-ev` | DRIVER, ADMIN | Check if EV |
| GET | `/health` | None | Health check |

---

## Vehicle Types

| Type | Description |
|------|-------------|
| `2W` | 2-Wheeler (motorbike, scooter) |
| `4W` | 4-Wheeler (car, SUV) |
| `HEAVY` | Heavy vehicle (truck, bus) |

---

## RBAC

| Action | DRIVER | ADMIN |
|--------|--------|-------|
| Register vehicle | ✅ own | ❌ |
| View own vehicles | ✅ | ✅ all |
| Update vehicle | ✅ own | ❌ |
| Delete vehicle | ✅ own | ✅ any |
| View all vehicles | ❌ | ✅ |

---

## RabbitMQ

### Published Events
| Event | Trigger |
|-------|---------|
| `VehicleRegisteredEvent` | New vehicle registered |
| `VehicleUpdatedEvent` | Vehicle details updated |
| `VehicleDeletedEvent` | Vehicle deleted |

### Consumed Events
| Event | From | Action |
|-------|------|--------|
| `DriverDeletedEvent` | auth-service | Cascade delete all driver vehicles |

---

## Running Locally

```bash
# Start dependencies
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
docker run -d --name postgres -p 5432:5432 -e POSTGRES_PASSWORD=yourpassword -e POSTGRES_DB=parkease_vehicle postgres:16

# Run migrations
dotnet ef migrations add InitialCreate --output-dir Migrations
dotnet ef database update

# Run
dotnet run
```

Swagger UI → http://localhost:5002

---

## Testing

**1. Get token from auth-service:**
```
POST http://localhost:5001/api/v1/auth/login
```

**2. Register a vehicle:**
```json
POST /api/v1/vehicles
Authorization: Bearer {token}
{
  "licensePlate": "MH12AB1234",
  "make": "Toyota",
  "model": "Corolla",
  "color": "White",
  "vehicleType": "4W",
  "isEV": false
}
```

**3. Get my vehicles:**
```
GET /api/v1/vehicles/my-vehicles
Authorization: Bearer {token}
```
