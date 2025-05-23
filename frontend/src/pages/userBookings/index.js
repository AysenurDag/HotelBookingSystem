// src/pages/user/UserBookingsPage.js
import "./UserBookings.css"; // Ek satır

import React, { useEffect, useState } from "react";
import {
  getBookingsByUserId,
  cancelBooking,
} from "../../services/bookingService";

const UserBookingsPage = () => {
  const [bookings, setBookings] = useState([]);
  const [error, setError] = useState(null);

  useEffect(() => {
    const userId = sessionStorage.getItem("userId");
    if (!userId) {
      setError("User not found in session.");
      return;
    }

    getBookingsByUserId(userId)
      .then((data) => setBookings(data))
      .catch((err) => setError(err.message));
  }, []);

  if (error) return <div>Error: {error}</div>;
  if (bookings.length === 0) return <div>No bookings found.</div>;

  const handleCancel = async (bookingId) => {
    try {
      const updatedBooking = await cancelBooking(bookingId);

      setBookings((prev) =>
        prev.map((b) => (b.bookingId === bookingId ? updatedBooking : b))
      );
    } catch (err) {
      alert("Cancellation failed: " + err.message);
    }
  };

  return (
    <div className="user-bookings-container">
      <h2>My Bookings</h2>
      <ul>
        {bookings.map((booking) => (
          <li key={booking.bookingId} className="booking-card">
            {/* Room ID satırı kaldırıldı */}
            <strong>Check-in:</strong> {booking.checkInDate} <br />
            <strong>Check-out:</strong> {booking.checkOutDate} <br />
            <strong>Status:</strong> {booking.status} <br />
            <strong>Amount:</strong> {booking.amount} {booking.currency} <br />
            <button
              className="cancel-button"
              onClick={() => handleCancel(booking.bookingId)}
            >
              Cancel Booking
            </button>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default UserBookingsPage;