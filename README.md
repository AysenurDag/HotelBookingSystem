# HotelBookingSystem

## Ozge's notes

## Aysenur's notes

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
