
package com.trivago.buse_booking_service.messaging.hotel_to_booking;

public class ReservationApprovedEvent {
    private Long bookingId;

    public ReservationApprovedEvent() {
    }

    public ReservationApprovedEvent(Long bookingId) {
        this.bookingId = bookingId;
    }

    public Long getBookingId() {
        return bookingId;
    }

    public void setBookingId(Long bookingId) {
        this.bookingId = bookingId;
    }

    @Override
    public String toString() {
        return "ReservationApprovedEvent{bookingId=" + bookingId + '}';
    }
}
