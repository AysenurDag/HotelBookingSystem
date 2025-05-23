booking: http://localhost:8081/swagger-ui/index.html

payment : http://localhost:8082/swagger/index.html

rabit : http://localhost:15672/#/queues

hotel : http://localhost:5050/

# HotelBookingSystem

This project implements a hotel management system using a microservices-based architecture. The system is designed to manage hotel listings, room bookings, user authentication, and payments with strong consistency and failure recovery using the Saga Pattern.

---

## Architecture Overview

The system is composed of four core microservices, each responsible for a distinct domain:

- **Authentication Service**: Manages user registration, login, JWT token generation, and role-based access control.  
- **Hotel Service**: Stores hotel metadata (details, amenities, ratings) and manages room inventory by date.  
- **Booking Service**: Handles reservation requests, booking status, and coordinates the Saga.  
- **Payment Service**: Processes payments, handles failures, and issues refunds.

A central **Saga Orchestrator** (implemented within the Booking Service) ensures distributed transactions across services, triggering compensating actions in case of failures.

---

## Services

### 🔐 Authentication Service
- **Aggregate**: `UserAggregate` (profiles, credentials)  
- **Features**:
  - Secure password hashing with ASP.NET Core Identity  
  - JWT access and refresh token workflows  
  - Role-based endpoint authorization

### 🏨 Hotel Service
- **Aggregates**:
  - `HotelAggregate` (hotel metadata)  
  - `RoomInventoryAggregate` (room availability, pricing)  
- **Features**:
  - Check and reserve room inventory  
  - Update availability on booking/cancellation  
  - Expose REST APIs for inventory queries

### 📆 Booking Service
- **Aggregate**: `BookingAggregate` (reservation details, status)  
- **Features**:
  - Create, confirm, cancel reservations  
  - Orchestrate Saga to ensure end-to-end consistency  
  - Expose endpoints for creating and managing bookings

### 💳 Payment Service
- **Aggregate**: `PaymentAggregate` (transaction status, history)  
- **Features**:
  - Simulate and process payment authorizations  
  - Publish success or failure events  
  - Handle refunds on cancellations

---

## Saga Pattern & Event Flow

The Saga Pattern is used to manage distributed transactions. Each step emits an event to RabbitMQ; the next service consumes it and acts accordingly. On failure, compensating events are published.

### Event Topics

| Topic/Queue                         | Description                                                    |
|-------------------------------------|----------------------------------------------------------------|
| `booking.created.queue`                   | Emitted by Booking Service when a reservation is requested.    |
| `booking.reservation.created.queue` | Consumed by Payment Service to initiate payment.               |
| `payment.succeeded.queue`                 | Emitted by Payment Service on successful payment.              |
| `payment.failed.queue`                    | Emitted by Payment Service on payment failure.                 |
| `reservation.approved.queue`        | Consumed by Booking Service to confirm reservation.            |
| `booking.cancelled.queue`                 | Emitted by Booking Service when user cancels.                  |
| `booking.refund.completed.queue`                  | Emitted by Payment Service after refund is completed.          |

### Step-by-Step Flow

1. **Reservation Requested**  
   - **Booking Service** creates a pending booking and publishes `booking.created`.  
   - Payload:  
     ```json
     {
       "reservationId": "...",
       "userId": "...",
       "hotelId": "...",
       "checkIn": "YYYY-MM-DD",
       "checkOut": "YYYY-MM-DD",
       "amount": 123.45
     }
     ```

2. **Payment Processing**  
   - **Payment Service** listens on `booking.reservation.created.queue`.  
   - Attempts authorization; then publishes either:  
     - `payment.succeeded` (on success)  
     - `payment.failed` (on failure)

3. **Booking Confirmation or Compensation**  
   - **Booking Service** listens on the result queues (`payment.succeeded`, `payment.failed`).  
   - On **success**:  
     - Publishes `reservation.approved.queue`.  
     - **Hotel Service** consumes this to finalize inventory decrement.  
   - On **failure**:  
     - Marks booking as cancelled.  

4. **Cancellation & Refund**  
   - When user cancels a confirmed booking, Booking Service publishes `booking.cancelled`.  
   - **Payment Service** consumes `booking.cancelled`, issues refund, and publishes `refund.processed`.  
   - **Booking Service** consumes `refund.processed`, updates reservation status to refunded, and triggers inventory restoration in Hotel Service.

---




## 🔁 Booking Saga Flow

The **Saga Pattern** is used to handle distributed transactions across services with compensation logic for failure scenarios.

### Step 1: Initiate Booking (Booking Service)

- User selects room and dates.
- Booking service creates a **pending booking record**.

### Step 2: Verify Room Availability (Hotel Service)

- Booking service requests room availability check.
- Hotel service verifies and **temporarily reserves** the room.

🛑 **Compensation**: If the booking fails, release the room reservation.

### Step 3: Process Payment (Payment Service)

- Booking service sends a request to process the payment.
- Payment service **authorizes the payment**.

🛑 **Compensation**: If the booking fails later, the payment is **voided or refunded**.

### Step 4: Confirm Booking (Booking Service)

- Upon successful payment:
  - Booking service **confirms the reservation**.
  - Hotel service **updates room inventory**.
  - Payment service **captures the authorized payment**.
  - Booking service **sends confirmation** to the user.

---

## ⚠️ Failure Handling

- Each step of the saga includes **compensation logic** to maintain system consistency.
- A **Saga Orchestrator** tracks the process and triggers compensating actions when necessary.


Monitoring & Logging

Prometheus: Service exposes /metrics endpoints for scraping metrics, alerting rules, and dashboarding.

Jaeger: Distributed tracing is enabled instrumentation; traces are collected in Jaeger for end-to-end request analysis across microservices.

Logging: Structured logging with Serilog, shipped to a centralized store for search and analysis.





## Ozge's notes

## Aysenur's notes

I have implemented the **authentication and authorization system** for handling secure access in the **Auth User Service**. The service manages user registration, login, JWT token generation, and role-based access control. It provides essential security infrastructure for the overall microservices ecosystem.

### Key Features

1. **User Registration and Authentication:**

   - Implemented user registration with secure password hashing using **ASP.NET Core Identity**.
   - Built a login mechanism that issues **JWT access tokens** and **refresh tokens**.
   - Stored user data securely using **Entity Framework Core** with a dedicated **AuthUserDbContext**.

2. **JWT Token Management:**

   - Generated **access tokens** that include user claims (ID, email, roles).
   - Supported **refresh tokens** to allow users to renew their session without re-authenticating.
   - Validated tokens using issuer, audience, expiration, and secret key validation.

3. **Role-Based Access Control (RBAC):**

   - Enabled assigning roles (e.g., "Admin", "User") to users during or after registration.
   - Restricted access to certain API endpoints based on assigned user roles.

4. **Event-Driven Architecture Support:**

   - Integrated **RabbitMQ** to publish user registration events for further processing by other services.
   - Promoted loose coupling and asynchronous communication across microservices.

5. **API Documentation:**

   - Configured **Swagger/OpenAPI** for clear and interactive API documentation.

6. **Security Best Practices:**
   - Enforced password policies: minimum length, complexity, and character types.
   - Implemented token expiration and refresh workflows to enhance security.

### **Expected Response (User Registration):**

```bash
{
  "id": "user-guid",
  "email": "user@example.com",
  "roles": ["User"]
}
```

This will trigger an event that can be consumed by other services if necessary:

```bash
📩 UserRegisteredEvent published: ID user-guid, Email user@example.com
```

## Nıgar's notes

I have implemented the **SAGA pattern** for handling the `ReservationCreatedEvent` in the **Payment Service**. The process simulates a payment transaction with success and failure scenarios. A consumer listens for the `ReservationCreatedEvent` and triggers the payment logic. Depending on the result, either a `PaymentSucceededEvent` or `PaymentFailedEvent` is published.

### Key Features

1. **ReservationCreatedEvent Handling:**

   - **Aggregate**: `PaymentAggregate` handles the domain events.
   - On receiving the `ReservationCreatedEvent`, the payment process is triggered.
   - The payment process is simulated, with a 50% chance of success or failure. Based on the result:
     - If successful: A `PaymentSucceededEvent` is published.
     - If failed: A `PaymentFailedEvent` is published.

2. **Test Consumer for Payment Simulation:**

   - A consumer (`ReservationCreatedConsumer`) listens to the `ReservationCreatedEvent`.
   - Upon receiving the event, it simulates the payment process and logs the success or failure.
   - The result of the simulation triggers either the `PaymentSucceededEvent` or `PaymentFailedEvent`.

3. **Message-Based Communication:**
   - **RabbitMQ** is used for event messaging between services:
     - **BookingService** sends the `ReservationCreatedEvent` to the `reservationQueue`.
     - **PaymentService** consumes the event and simulates payment processing.

### **Expected Response:**

```bash
{
  "id": 1,
  "hotelId": "hotel-123",
  "userId": "user-456",
  "checkInDate": "2025-05-01",
  "checkOutDate": "2025-05-05"
}
```

This will trigger the payment simulation, if payment is successful:

```bash
📩 ReservationReceived: ID 1, Hotel: hotel-123
💳 Processing payment for User: user-456, Hotel: hotel-123
✅ Payment successful for reservation ID: 1
📤 Event published to payment_succeeded: {"reservationId":1,"userId":"user-456","hotelId":"hotel-123"}
```

## Buse's notes

I implemented a test **SAGA pattern** for handling the **ReservationCreatedEvent** in the **Booking Service**. This process simulates a payment transaction with success and failure scenarios. Additionally, the test consumer was created to simulate the payment process, and necessary Docker containers for MySQL and RabbitMQ were set up.Nigar will continue for payment.

---

## Key Features

### 1. **ReservationCreatedEvent Handling**:

Aggregate - Reservation
Domain Event - ReservationCreatedEvent

- Implemented a test **SAGA pattern** for handling `ReservationCreatedEvent` in the `Booking Service`.
- Upon receiving the event, a **payment simulation** is triggered.
- The payment simulation has a 50% chance of success or failure. Based on the result:
  - If successful: **Payment succeeded** is logged.
  - If failed: **Payment failed** is logged.

### 2. **Test Consumer for Payment Simulation**:

- Created a **dummy test consumer** that listens for the `ReservationCreatedEvent` and simulates a payment process.
- Based on the success/failure of the simulated payment, an appropriate message is logged (success or failure).

### 3. **Docker Setup**:

- Configured **MySQL** container for database operations:
  - Database: `booking_db`
  - Username: `root`
  - Password: `123456`
- Configured **RabbitMQ** container for event messaging between services.

---

## Testing with Curl

To simulate the `ReservationCreatedEvent`, you can use the following **`curl`** commands to send HTTP requests:

### 1. **Send a Reservation Created Event**

```bash
curl -X POST http://localhost:8080/reservations \
-H "Content-Type: application/json" \
-d '{
  "hotelId": "hotel-123",
  "userId": "user-456",
  "checkInDate": "2025-05-01",
  "checkOutDate": "2025-05-05"
}'
```

### **Expected Response:**

```bash
{
  "id": 1,
  "hotelId": "hotel-123",
  "userId": "user-456",
  "checkInDate": "2025-05-01",
  "checkOutDate": "2025-05-05"
}
```

This will trigger the payment simulation, and you should see the log output like:

```bash
📩 ReservationCreatedEvent received. Reservation ID: 1
💳 Processing payment for User: user-456, Hotel: hotel-123
❌ Payment failed for reservation ID: 1
```


### **MONITORING**


![Screenshot 2025-05-24 014812](https://github.com/user-attachments/assets/68d26868-652d-4ed5-b57b-44f636a6582c)
![Screenshot 2025-05-24 020437](https://github.com/user-attachments/assets/854a98a4-4eb8-4503-a99b-e80ce2e87f45)



