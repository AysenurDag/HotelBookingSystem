package com.trivago.buse_booking_service.messaging.hotel_to_booking;

public class ReservationApprovedEvent {
    private Long bookingId;
    private String status; // örneğin "approved"

    public Long getBookingId() {
        return bookingId;
    }

    public void setBookingId(Long bookingId) {
        this.bookingId = bookingId;
    }

    public String getStatus() {
        return status;
    }

    public void setStatus(String status) {
        this.status = status;
    }
}
