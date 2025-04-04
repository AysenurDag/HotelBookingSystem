# HotelBookingSystem

## Buse

- database: `booking_db`
- username: `root`
- password: `123456`

```json
{
  "reservationId": 1,
  "hotelId": "hotel-123",
  "userId": "user-456"
}
```

## 🌐 REST API

### `POST /reservations`

Create a new reservation.

```json
{
  "hotelId": "hotel-123",
  "userId": "user-456",
  "checkInDate": "2025-05-01",
  "checkOutDate": "2025-05-05"
}
```

Returns saved reservation + triggers event.

---
