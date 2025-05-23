package com.trivago.buse_booking_service.messaging.payment_to_booking;

public class PaymentSucceededEvent {

    private Long bookingId;
    private Long paymentId;

    public PaymentSucceededEvent() {
    }

    public PaymentSucceededEvent(Long bookingId, Long paymentId) {
        this.bookingId = bookingId;
        this.paymentId = paymentId;
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
}
