package com.trivago.buse_booking_service.messaging;

/**
 * Hotel Service tarafından gönderilir.
 * Oda uygun değilse Booking Service tarafından dinlenir.
 */
public class RoomAvailabilityFailedEvent {

    private String bookingId;
    private String reason;

    public RoomAvailabilityFailedEvent() {
    }

    public RoomAvailabilityFailedEvent(String bookingId, String reason) {
        this.bookingId = bookingId;
        this.reason = reason;
    }

    public String getBookingId() {
        return bookingId;
    }

    public void setBookingId(String bookingId) {
        this.bookingId = bookingId;
    }

    public String getReason() {
        return reason;
    }

    public void setReason(String reason) {
        this.reason = reason;
    }
}
