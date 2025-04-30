package com.trivago.buse_booking_service.messaging;

public class PaymentSucceededEvent {
    private String bookingId;
    private String paymentId;

    public PaymentSucceededEvent() {
    }

    public PaymentSucceededEvent(String bookingId, String paymentId) {
        this.bookingId = bookingId;
        this.paymentId = paymentId;
    }

    public String getBookingId() {
        return bookingId;
    }

    public void setBookingId(String bookingId) {
        this.bookingId = bookingId;
    }

    public String getPaymentId() {
        return paymentId;
    }

    public void setPaymentId(String paymentId) {
        this.paymentId = paymentId;
    }
}
