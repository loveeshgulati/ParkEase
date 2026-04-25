# ParkEase.Notification — Notification Microservice

## Overview
Handles all real-time and persistent notifications across the ParkEase platform.
Consumes events from ALL other services via RabbitMQ and pushes real-time
alerts via SignalR.
Runs on **port 5007**.

---

## Project Structure

```
ParkEase.Notification/
├── Controllers/
│   └── NotificationController.cs
├── Consumers/
│   └── NotificationConsumers.cs   ← 17 consumers for all services
├── Data/
│   └── NotificationDbContext.cs
├── DTOs/
│   └── NotificationDtos.cs
├── Entities/
│   └── Notification.cs
├── Events/
│   └── NotificationEvents.cs      ← all event contracts
├── Hubs/
│   └── NotificationHub.cs         ← SignalR hub
├── Interfaces/
│   └── INotificationInterfaces.cs
├── Middleware/
│   └── JwtMiddleware.cs
├── Migrations/
├── Repositories/
│   └── NotificationRepository.cs
├── Services/
│   └── NotificationService.cs
├── appsettings.json
├── appsettings.Development.json
├── docker-compose.yml
├── Dockerfile
└── ParkEase.Notification.csproj
```

---

## Notification Types

| Type | Trigger |
|------|---------|
| `WELCOME` | New user registration |
| `APPROVAL` | Manager/lot approved, account reactivated |
| `REJECTION` | Manager/lot rejected |
| `SUSPENSION` | Account suspended |
| `BOOKING` | Booking created, cancelled, extended |
| `CHECKIN` | Driver checks in |
| `CHECKOUT` | Driver checks out |
| `EXPIRY` | Booking auto-expired |
| `PAYMENT` | Payment processed |
| `REFUND` | Refund processed |
| `PROMO` | Admin broadcast |

---

## API Endpoints

### All Roles (own notifications only)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/notifications` | All own notifications |
| GET | `/api/v1/notifications/unread` | Unread only |
| GET | `/api/v1/notifications/unread-count` | Badge count |
| PUT | `/api/v1/notifications/{id}/read` | Mark as read |
| PUT | `/api/v1/notifications/read-all` | Mark all as read |
| DELETE | `/api/v1/notifications/{id}` | Delete notification |

### Admin Only
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/notifications/broadcast` | Broadcast to users |
| GET | `/api/v1/notifications/all` | All platform notifications |

---

## SignalR — Real-time Notifications

```
Connect to: ws://localhost:5008/hubs/notifications?access_token={jwt}

Client receives: "ReceiveNotification" event with:
{
  "notificationId": 1,
  "title": "Booking Confirmed",
  "message": "Booking #5 confirmed",
  "type": "BOOKING",
  "sentAt": "2024-01-01T10:00:00Z"
}
```

---

## RabbitMQ Consumers (17 total)

| Consumer | From | Notification Sent To |
|----------|------|---------------------|
| `UserRegisteredConsumer` | auth | Driver/Manager (welcome) |
| `ManagerApprovedConsumer` | auth | Manager (approved) |
| `ManagerRejectedConsumer` | auth | Manager (rejected) |
| `ManagerSuspendedConsumer` | auth | Manager (suspended) |
| `ManagerReactivatedConsumer` | auth | Manager (reactivated) |
| `DriverSuspendedConsumer` | auth | Driver (suspended) |
| `DriverReactivatedConsumer` | auth | Driver (reactivated) |
| `LotApprovedConsumer` | parkinglot | Manager (lot approved) |
| `LotRejectedConsumer` | parkinglot | Manager (lot rejected) |
| `BookingCreatedConsumer` | booking | Driver (confirmed) |
| `BookingCancelledConsumer` | booking | Driver (cancelled) |
| `BookingCheckedInConsumer` | booking | Driver (checked in) |
| `BookingCheckedOutConsumer` | booking | Driver (checked out) |
| `BookingExpiredConsumer` | booking | Driver (expired) |
| `BookingExtendedConsumer` | booking | Driver (extended) |
| `PaymentProcessedConsumer` | payment | Driver (payment receipt) |
| `RefundProcessedConsumer` | payment | Driver (refund) |

---

## Running Locally

```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
docker run -d --name postgres -p 5432:5432 -e POSTGRES_PASSWORD=yourpassword -e POSTGRES_DB=parkease_notification postgres:16

dotnet ef migrations add InitialCreate --output-dir Migrations
dotnet ef database update
dotnet run
```

Swagger UI  → http://localhost:5008
SignalR Hub → ws://localhost:5008/hubs/notifications
