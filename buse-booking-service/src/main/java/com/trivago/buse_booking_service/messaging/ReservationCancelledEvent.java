package com.trivago.buse_booking_service.messaging;

public class ReservationCancelledEvent {

    private Long bookingId;
    private String reason;

    // Constructors
    public ReservationCancelledEvent() {}

    public ReservationCancelledEvent(Long bookingId, String reason) {
        this.bookingId = bookingId;
        this.reason = reason;
    }

    // Getters and Setters
    public Long getBookingId() { return bookingId; }
    public void setBookingId(Long bookingId) { this.bookingId = bookingId; }

    public String getReason() { return reason; }
    public void setReason(String reason) { this.reason = reason; }
}
