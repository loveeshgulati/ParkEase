# ParkEase Backend Microservices

This is the backend for the ParkEase application, built using **.NET 8** and a **Microservices Architecture**.

## System Use Case Diagram

The following diagram outlines the overarching use cases across the entire ParkEase platform, mapping all system actors (Guest, Driver, Manager, Admin, System) to their respective capabilities.

![ParkEase Use Case Diagram](./docs/images/use-case-diagram.png)

## Microservices Architecture & Connectivity


```mermaid
graph TD
    Client[Angular Frontend]

    subgraph "ParkEase Microservices (.NET 8)"
        Auth[Auth Service<br/>:7002]
        Booking[Booking Service<br/>:5001]
        Spot[Spot Service<br/>:5002]
        ParkingLot[ParkingLot Service<br/>:5003]
        Vehicle[Vehicle Service<br/>:7004]
        Payment[Payment Service<br/>:5006]
        Notification[Notification Service<br/>:5008]
    end

    subgraph "Databases (PostgreSQL)"
        DB_Auth[(Auth DB)]
        DB_Vehicle[(Vehicle DB)]
        DB_Lot[(ParkingLot DB)]
        DB_Spot[(Spot DB)]
        DB_Booking[(Booking DB)]
        DB_Payment[(Payment DB)]
        DB_Notification[(Notification DB)]
    end

    subgraph "Message Broker"
        RMQ{{RabbitMQ Event Bus}}
    end

    Client --> Auth
    Client --> Vehicle
    Client --> ParkingLot
    Client --> Spot
    Client --> Booking
    Client --> Payment
    Client --> Notification

    Auth --> DB_Auth
    Vehicle --> DB_Vehicle
    ParkingLot --> DB_Lot
    Spot --> DB_Spot
    Booking --> DB_Booking
    Payment --> DB_Payment
    Notification --> DB_Notification

    Auth -.->|Publish/Subscribe| RMQ
    Vehicle -.->|Publish/Subscribe| RMQ
    ParkingLot -.->|Publish/Subscribe| RMQ
    Spot -.->|Publish/Subscribe| RMQ
    Booking -.->|Publish/Subscribe| RMQ
    Payment -.->|Publish/Subscribe| RMQ
    Notification -.->|Publish/Subscribe| RMQ
```

## Running Locally

1. Make sure you have **PostgreSQL** and **RabbitMQ** running.
2. Update the connection strings in the respective `appsettings.json` files if necessary.
3. Run the services from the command line:
   ```bash
   dotnet run --project ParkEase.Auth/ParkEase.Auth.csproj
   # Repeat for other services
   ```
   Or use the provided solution file `ParkEase.sln` to run in Visual Studio or Rider.

## Testing & Quality

- **Unit Tests**: Run tests with `dotnet test ParkEase.Tests/ParkEase.Tests.csproj`
- **SonarQube**: Run the `sonar-scan-backend.ps1` script to analyze the project with SonarQube.

---

## Microservice Internal Workflows (File-to-File Flow)

The following diagrams illustrate the internal execution flow of files within each microservice when an API request is received. The architecture follows a strict Controller -> Service -> Repository -> Database pattern to ensure separation of concerns.

### 1. Auth Service Flow
```mermaid
graph TD
    Router[HTTP Request] --> C[Controllers<br/>AuthController.cs / AdminController.cs]
    C --> S[Services<br/>AuthService.cs / AdminService.cs]
    S --> R[Repositories<br/>UserRepository.cs]
    R --> DB_CTX[Data<br/>AuthDbContext.cs]
    DB_CTX --> DB[(PostgreSQL<br/>parkease_auth)]
```

### 2. Booking Service Flow
```mermaid
graph TD
    Router[HTTP Request] --> C[Controllers<br/>BookingController.cs]
    C --> S[Services<br/>BookingService.cs]
    S --> HTTP[HTTP Clients<br/>SpotHttpClient.cs]
    S --> R[Repositories<br/>BookingRepository.cs]
    R --> DB_CTX[Data<br/>BookingDbContext.cs]
    DB_CTX --> DB[(PostgreSQL<br/>parkease_booking)]
    S -.->|Publish BookingCreated| MQ[RabbitMQ / MassTransit]
```

### 3. Spot Service Flow
```mermaid
graph TD
    Router[HTTP Request] --> C[Controllers<br/>SpotController.cs]
    C --> S[Services<br/>SpotService.cs]
    S --> R[Repositories<br/>SpotRepository.cs]
    R --> DB_CTX[Data<br/>SpotDbContext.cs]
    DB_CTX --> DB[(PostgreSQL<br/>parkease_spot)]
    S -.->|Publish SpotUpdated| MQ[RabbitMQ / MassTransit]
```

### 4. ParkingLot Service Flow
```mermaid
graph TD
    Router[HTTP Request] --> C[Controllers<br/>ParkingLotController.cs]
    C --> S[Services<br/>ParkingLotService.cs]
    S --> R[Repositories<br/>ParkingLotRepository.cs]
    R --> DB_CTX[Data<br/>ParkingLotDbContext.cs]
    DB_CTX --> DB[(PostgreSQL<br/>parkease_lot)]
    MQ[RabbitMQ Event] -.->|Consume SpotCountUpdated| Consumer[Consumers<br/>SpotCountUpdatedConsumer.cs]
    Consumer --> S
```

### 5. Payment Service Flow
```mermaid
graph TD
    Router[HTTP Request] --> C[Controllers<br/>PaymentController.cs / WebhookController.cs]
    C --> S[Services<br/>PaymentService.cs]
    S --> Gateway[Payment Gateway<br/>Razorpay Integration]
    S --> R[Repositories<br/>PaymentRepository.cs]
    R --> DB_CTX[Data<br/>PaymentDbContext.cs]
    DB_CTX --> DB[(PostgreSQL<br/>parkease_payment)]
    S -.->|Publish PaymentCompleted| MQ[RabbitMQ / MassTransit]
```

### 6. Vehicle Service Flow
```mermaid
graph TD
    Router[HTTP Request] --> C[Controllers<br/>VehicleController.cs]
    C --> S[Services<br/>VehicleService.cs]
    S --> R[Repositories<br/>VehicleRepository.cs]
    R --> DB_CTX[Data<br/>VehicleDbContext.cs]
    DB_CTX --> DB[(PostgreSQL<br/>parkease_vehicle)]
```

### 7. Notification Service Flow
```mermaid
graph TD
    Router[HTTP Request] --> C[Controllers<br/>NotificationController.cs]
    C --> S[Services<br/>NotificationService.cs]
    
    MQ[RabbitMQ Events] -.->|Consume| Cons[Consumers<br/>BookingCreatedConsumer.cs, etc.]
    Cons --> S
    
    S --> Hub[SignalR Hubs<br/>NotificationHub.cs]
    Hub -.->|Push to Client| Client[Angular Frontend]
    
    S --> R[Repositories<br/>NotificationRepository.cs]
    R --> DB_CTX[Data<br/>NotificationDbContext.cs]
    DB_CTX --> DB[(PostgreSQL<br/>parkease_notification)]
```
---

## Data Architecture (ER Diagram)

The following diagram illustrates the logical relationships between entities across the different microservice databases.

```mermaid
erDiagram
    USER ||--o{ VEHICLE : "owns"
    USER ||--o{ BOOKING : "creates"
    USER ||--o{ PARKING_LOT : "manages (if Manager)"
    
    PARKING_LOT ||--o{ PARKING_SPOT : "contains"
    PARKING_LOT ||--o{ BOOKING : "hosts"
    
    PARKING_SPOT ||--o{ BOOKING : "assigned to"
    
    BOOKING ||--|| PAYMENT : "generates"
    BOOKING ||--o{ NOTIFICATION : "triggers"

    USER {
        int UserId PK
        string FullName
        string Email
        string Role
        string Status
    }

    VEHICLE {
        int VehicleId PK
        int OwnerId FK
        string LicensePlate
        string VehicleType
    }

    PARKING_LOT {
        int LotId PK
        int ManagerId FK
        string Name
        string Status
        int TotalSpots
    }

    PARKING_SPOT {
        int SpotId PK
        int LotId FK
        string SpotNumber
        string SpotType
        string Status
    }

    BOOKING {
        int BookingId PK
        int UserId FK
        int LotId FK
        int SpotId FK
        string Status
        datetime CheckInTime
    }

    PAYMENT {
        int PaymentId PK
        int BookingId FK
        decimal Amount
        string Status
    }

    NOTIFICATION {
        int NotificationId PK
        int UserId FK
        string Type
        string Message
    }
```

---

## Low-Level Design (LLD) Architectural Diagrams

### 1. Structural Layer-by-Layer Execution Pipeline

All 7 backend microservices follow a clean, decoupled dependency flow designed to ensure strict separation of concerns:

```mermaid
classDiagram
    direction TB
    
    class HTTPRequest {
        <<HTTP Request>>
    }

    class Controllers {
        +ControllerBase
        +Inject Service Interface
        +Expose API Routes
        +Filter Role Permissions
    }

    class Services {
        +Inject Repository Interface
        +Inject IPublishEndpoint (MassTransit)
        +Implement Business Use-Cases
        +Compute Calculations & Logic
    }

    class Repositories {
        +Inject DbContext
        +Query Database Records
        +Perform CRUD Operations
    }

    class DbContext {
        +DbSet DbSet
        +Configure Schema Mappings
        +Apply Entity Constraints
    }

    class Database {
        <<PostgreSQL Database Schema>>
    }

    class MassTransitBus {
        <<RabbitMQ Event Bus>>
    }

    class Consumers {
        +Consume(Context)
        +Inject Service Interface
    }

    HTTPRequest --> Controllers : Invokes Route
    Controllers --> Services : Calls Business Logic Interface
    Services --> Repositories : Calls Data Query Interface
    Services --> MassTransitBus : Publishes Async Events (Publish)
    Repositories --> DbContext : Interfaces with EF Core
    DbContext --> Database : Connects to Logical Schema
    MassTransitBus --> Consumers : Dispatches Subscribed Events
    Consumers --> Services : Triggers Business Cascades
```

---

### 2. Microservice Dynamic Workflow Sequences

Below are the low-level sequence designs illustrating how the main platform operations run asynchronously across the network.

#### A. Booking Reservation & Spot Allocation Workflow

When a Driver makes a parking reservation on the frontend client, the booking microservice coordinates spot status changes and triggers notification pushes:

```mermaid
sequenceDiagram
    autonumber
    actor Driver as Driver Client
    participant BookCtrl as BookingController
    participant BookSvc as BookingService
    participant SpotHTTP as SpotHttpClient
    participant SpotAPI as Spot Microservice
    participant Bus as RabbitMQ / MassTransit
    participant NotifCons as BookingCreatedConsumer
    participant NotifHub as NotificationHub (SignalR)

    Driver->>BookCtrl: POST /api/v1/bookings (LotId, SpotId, Duration)
    BookCtrl->>BookSvc: CreateBookingAsync(userId, DTO)
    BookSvc->>SpotHTTP: GetSpotAsync(spotId)
    Note over SpotHTTP: Propagates current JWT authorization header
    SpotHTTP->>SpotAPI: GET /api/v1/spots/{id}
    SpotAPI-->>SpotHTTP: Returns Spot Details & Price
    
    alt Spot is AVAILABLE
        BookSvc->>SpotHTTP: ReserveSpotAsync(spotId)
        SpotHTTP->>SpotAPI: PUT /api/v1/spots/{id}/reserve
        SpotAPI-->>SpotHTTP: Returns 200 OK (Status changed to RESERVED)
        BookSvc->>BookSvc: Save Reservation Details (Status = RESERVED)
        BookSvc->>Bus: Publish(BookingCreatedEvent)
        BookSvc-->>BookCtrl: Returns BookingDto
        BookCtrl-->>Driver: Returns 201 Created Status
    else Spot is OCCUPIED / RESERVED
        SpotAPI-->>SpotHTTP: Returns Spot Unavailable (400 Bad Request)
        BookSvc-->>Driver: Returns 400 Bad Request Exception
    end

    %% Async notification route
    Bus->>NotifCons: Consume(BookingCreatedEvent)
    NotifCons->>NotifHub: SendAsync(RecipientId, Title, Message)
    NotifHub-->>Driver: WebSocket PUSH: "Booking Confirmed 🅿️"
```

#### B. Check-Out & Payment Verification Flow

Upon vehicle departure, the checkout pipeline calculates the parking duration, initiates a payment gateway handshake, and verifies cryptographic signatures:

```mermaid
sequenceDiagram
    autonumber
    actor Driver as Driver Client
    participant BookCtrl as BookingController
    participant BookSvc as BookingService
    participant PaySvc as PaymentService
    participant RazorAPI as Razorpay Payment Gateway
    participant SpotHTTP as SpotHttpClient
    participant SpotAPI as Spot Microservice
    participant Bus as RabbitMQ / MassTransit

    Driver->>BookCtrl: PUT /api/v1/bookings/{id}/checkout
    BookCtrl->>BookSvc: CheckOutAsync(bookingId)
    BookSvc->>BookSvc: Calculate Duration & Charge (PricePerHr * Hrs)
    BookSvc->>BookSvc: Save Booking State (Status = COMPLETED)
    BookSvc->>SpotHTTP: ReleaseSpotAsync(spotId)
    SpotHTTP->>SpotAPI: PUT /api/v1/spots/{id}/release (Status = AVAILABLE)
    BookSvc->>Bus: Publish(BookingCheckedOutEvent)
    BookSvc-->>Driver: Returns checkout payload (TotalAmount)

    %% Payment Flow
    Driver->>PaySvc: POST /api/v1/payments/create-order (Amount)
    PaySvc->>RazorAPI: Create Razorpay Order
    RazorAPI-->>PaySvc: Return OrderId & Credentials
    PaySvc-->>Driver: Return Payment Order JSON
    Note over Driver: Razorpay checkout modal opens in Angular UI
    Driver->>Driver: complete payment transaction
    Driver->>PaySvc: POST /api/v1/payments/process (OrderId, PaymentId, Signature)
    PaySvc->>PaySvc: Verify SHA256 HMAC signature with KeySecret
    PaySvc->>PaySvc: Save Transaction Details (Status = PAID)
    PaySvc->>Bus: Publish(PaymentProcessedEvent)
    PaySvc-->>Driver: Returns 201 Success
```

---

### 3. Database Schema Isolation Architecture

All 7 microservices share a single Postgres instance, partitioned logically using dedicated schemas to prevent data collisions and enforce isolated migration boundaries:

| Microservice | Target Postgres Schema | Main Domain Table | Migration Tracking Table |
| :--- | :--- | :--- | :--- |
| **Auth Service** | `auth` | `auth.users` | `auth.__EFMigrationsHistory_Auth` |
| **Vehicle Service** | `vehicle` | `vehicle.vehicles` | `vehicle.__EFMigrationsHistory_Vehicle` |
| **ParkingLot Service** | `parking` | `parking.parking_lots` | `parking.__EFMigrationsHistory_ParkingLot` |
| **Spot Service** | `spot` | `spot.parking_spots` | `spot.__EFMigrationsHistory_Spot` |
| **Booking Service** | `booking` | `booking.bookings` | `booking.__EFMigrationsHistory_Booking` |
| **Payment Service** | `payment` | `payment.payments` | `payment.__EFMigrationsHistory_Payment` |
| **Notification Service**| `notification` | `notification.notifications`| `notification.__EFMigrationsHistory_Notification` |

---
