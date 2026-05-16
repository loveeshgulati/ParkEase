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
