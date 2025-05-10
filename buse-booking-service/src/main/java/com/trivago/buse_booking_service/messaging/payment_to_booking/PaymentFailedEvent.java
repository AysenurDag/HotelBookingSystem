package com.trivago.buse_booking_service.messaging.payment_to_booking;

public class PaymentFailedEvent {

    private Long bookingId;
    private Long paymentId;
    private String reason;

    public PaymentFailedEvent() {
    }

    public PaymentFailedEvent(Long bookingId, Long paymentId, String reason) {
        this.bookingId = bookingId;
        this.paymentId = paymentId;
        this.reason = reason;
    }

    public Long getBookingId() {
        return bookingId;
    }

    public void setBookingId(Long bookingId) {
        this.bookingId = bookingId;
    }

    public Long getPaymentId() {
        return paymentId;
    }

    public void setPaymentId(Long paymentId) {
        this.paymentId = paymentId;
    }

    public String getReason() {
        return reason;
    }

    public void setReason(String reason) {
        this.reason = reason;
    }
}
