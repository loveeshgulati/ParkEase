# ParkEase.Payment — Payment Management Microservice

## Overview
Handles all payment processing, refunds, receipts and revenue reporting.
Runs on **port 5006**. Uses JWT from auth-service.

---

## Project Structure

```
ParkEase.Payment/
├── Controllers/
│   └── PaymentController.cs
├── Consumers/
│   └── PaymentConsumers.cs     ← BookingCheckedOut + BookingCancelled
├── Data/
│   └── PaymentDbContext.cs
├── DTOs/
│   └── PaymentDtos.cs
├── Entities/
│   └── Payment.cs
├── Events/
│   └── PaymentEvents.cs
├── Interfaces/
│   └── IPaymentInterfaces.cs   ← IPaymentRepository + IPaymentService
├── Middleware/
│   └── JwtMiddleware.cs
├── Migrations/
├── Repositories/
│   └── PaymentRepository.cs
├── Services/
│   └── PaymentService.cs
├── appsettings.json
├── appsettings.Development.json
└── ParkEase.Payment.csproj
```

---

## Payment Status Flow

```
Booking checkout → PENDING (auto-created by consumer)
Driver pays      → PAID
Cancellation     → REFUNDED (auto-processed if eligible)
Gateway fail     → FAILED
```

---

## Payment Modes

| Mode | Description |
|------|-------------|
| `CARD` | Credit/Debit card |
| `UPI` | UPI payment |
| `WALLET` | ParkEase wallet balance |
| `CASH` | Cash on exit |

---

## API Endpoints

### Driver
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/payments/process` | Process payment |
| GET | `/api/v1/payments/my-payments` | Own payment history |
| GET | `/api/v1/payments/{id}` | Single payment |
| GET | `/api/v1/payments/booking/{bookingId}` | Payment by booking |
| POST | `/api/v1/payments/refund` | Request refund |
| GET | `/api/v1/payments/{id}/receipt` | Download receipt |

### Manager
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/payments/revenue/{lotId}` | Lot revenue by date range |

### Admin
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/payments/all` | All platform payments |
| GET | `/api/v1/payments/platform/revenue` | Platform-wide revenue |

---

## RabbitMQ

### Published Events
| Event | Trigger |
|-------|---------|
| `PaymentProcessedEvent` | Payment completed |
| `RefundProcessedEvent` | Refund processed |
| `PaymentFailedEvent` | Payment failed |

### Consumed Events
| Event | From | Action |
|-------|------|--------|
| `BookingCheckedOutEvent` | booking-service | Auto-create PENDING payment |
| `BookingCancelledEvent` | booking-service | Auto-process refund if eligible |

---

## Running Locally

```bash
# Make sure you have PostgreSQL and RabbitMQ running locally
dotnet ef migrations add InitialCreate --output-dir Migrations
dotnet ef database update
dotnet run
```

Swagger UI → http://localhost:5006

---

## Testing Flow

**1. Driver checks out (booking-service creates PENDING payment)**

**2. Driver processes payment:**
```json
POST /api/v1/payments/process
Authorization: Bearer {driver_token}
{
  "bookingId": 1,
  "mode": "UPI",
  "amount": 150.00
}
```

**3. Get receipt:**
```
GET /api/v1/payments/1/receipt
```

**4. Manager views revenue:**
```
GET /api/v1/payments/revenue/1?from=2024-01-01&to=2024-12-31
```
