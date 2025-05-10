package com.trivago.buse_booking_service.messaging.booking_to_payment;

/**
 * Saga başarısız olduğunda gönderilir.
 * Audit/Logging/Notification Service bu event'i dinleyebilir.
 */
public class ReservationCancelledEvent {

    private Long bookingId;
    private String reason;

    public ReservationCancelledEvent() {
    }

    public ReservationCancelledEvent(Long bookingId, String reason) {
        this.bookingId = bookingId;
        this.reason = reason;
    }

    public Long getBookingId() {
        return bookingId;
    }

    public void setBookingId(Long bookingId) {
        this.bookingId = bookingId;
    }

    public String getReason() {
        return reason;
    }

    public void setReason(String reason) {
        this.reason = reason;
    }
}
