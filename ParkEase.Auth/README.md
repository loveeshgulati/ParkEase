# ParkEase.Auth — Authentication & Admin Microservice

## Overview
The Auth service is the **security gateway** for the entire ParkEase platform.
Handles user registration, login, JWT tokens, RBAC, and full admin control
over drivers and managers.

---

## Project Structure

```
ParkEase.Auth/
├── Controllers/
│   ├── AuthController.cs          ← register, login, profile (all roles)
│   └── AdminController.cs         ← admin manages drivers + managers
├── Consumers/
│   └── UserDeactivationRolledBackConsumer.cs
├── Data/
│   └── AuthDbContext.cs
├── DTOs/
│   └── AuthDtos.cs
├── Entities/
│   ├── User.cs
│   └── AuditLog.cs
├── Events/
│   └── AuthEvents.cs
├── Interfaces/
│   ├── IUserRepository.cs
│   ├── IAuditLogRepository.cs
│   └── IAuthService.cs            ← IAuthService + IAdminService
├── Middleware/
│   └── JwtMiddleware.cs
├── Migrations/
├── Repositories/
│   ├── UserRepository.cs
│   └── AuditLogRepository.cs
├── Sagas/
│   └── AccountDeactivationSaga.cs
├── Services/
│   ├── AuthService.cs
│   └── AdminService.cs
├── appsettings.json
├── appsettings.Development.json
└── ParkEase.Auth.csproj
```

---

## Roles

| Role | Signup | Approval | Access |
|------|--------|----------|--------|
| ADMIN | Seeded in DB | None | Full platform |
| MANAGER | Self-register | ✅ Admin must approve | Own lots only |
| DRIVER | Self-register | ❌ Instant access | Own bookings only |

---

## Default Admin Credentials
```
Email:    admin@parkease.com
Password: Admin@123
```
> Change this in production!

---

## API Endpoints

### Public
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/auth/register` | Register Driver or Manager |
| POST | `/api/v1/auth/login` | Login |
| POST | `/api/v1/auth/refresh` | Refresh token |
| GET | `/api/v1/auth/validate` | Validate JWT token |

### Authenticated (Any Role)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/auth/logout` | Logout |
| GET | `/api/v1/auth/profile` | Get own profile |
| PUT | `/api/v1/auth/profile` | Update own profile |
| PUT | `/api/v1/auth/password` | Change password |
| DELETE | `/api/v1/auth/deactivate` | Deactivate own account |

### Admin Only
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/admin/managers/pending` | Pending manager requests |
| GET | `/api/v1/admin/managers` | All managers |
| GET | `/api/v1/admin/managers/{id}` | Single manager |
| PUT | `/api/v1/admin/managers/{id}/approve` | Approve manager |
| PUT | `/api/v1/admin/managers/{id}/reject` | Reject manager |
| PUT | `/api/v1/admin/managers/{id}/suspend` | Suspend manager |
| PUT | `/api/v1/admin/managers/{id}/reactivate` | Reactivate manager |
| DELETE | `/api/v1/admin/managers/{id}` | Delete manager |
| GET | `/api/v1/admin/drivers` | All drivers |
| GET | `/api/v1/admin/drivers/{id}` | Single driver |
| PUT | `/api/v1/admin/drivers/{id}/suspend` | Suspend driver |
| PUT | `/api/v1/admin/drivers/{id}/reactivate` | Reactivate driver |
| DELETE | `/api/v1/admin/drivers/{id}` | Delete driver |
| GET | `/api/v1/admin/users` | All users |
| GET | `/api/v1/admin/users/{id}` | Any user |

---

## Running Locally

### Prerequisites
- PostgreSQL database running locally or accessible
- RabbitMQ server running locally

### Setup
```bash
# Run migrations
dotnet ef database update

# Start the service
dotnet run
```
- Auth Service → http://localhost:7003 (Swagger available)

---

## Saga: AccountDeactivationSaga
```
Step 1: auth-service     → deactivates user
Step 2: booking-service  → cancels active bookings  ← cross-service
Step 3: notification     → notifies user            ← cross-service

Compensation if Step 2 fails:
→ UserDeactivationRolledBackEvent
→ User reactivated automatically
```

---

## RabbitMQ Events Published

| Event | Trigger | Consumer |
|-------|---------|----------|
| `UserRegisteredEvent` | New registration | notification-service |
| `ManagerSignupRequestedEvent` | Manager registers | notification-service (admin alert) |
| `ManagerApprovedEvent` | Admin approves | notification-service |
| `ManagerRejectedEvent` | Admin rejects | notification-service |
| `ManagerSuspendedEvent` | Admin suspends | notification-service |
| `ManagerReactivatedEvent` | Admin reactivates | notification-service |
| `ManagerDeletedEvent` | Admin deletes | parkinglot-service (cascade) |
| `DriverSuspendedEvent` | Admin suspends | notification-service |
| `DriverReactivatedEvent` | Admin reactivates | notification-service |
| `DriverDeletedEvent` | Admin deletes | booking-service (cascade) |
| `UserDeactivatedEvent` | Self-deactivate | AccountDeactivationSaga |
| `UserProfileUpdatedEvent` | Profile update | vehicle-service, booking-service |
