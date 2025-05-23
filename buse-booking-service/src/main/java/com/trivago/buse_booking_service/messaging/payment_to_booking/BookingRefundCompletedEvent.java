
package com.trivago.buse_booking_service.messaging.payment_to_booking;

import com.fasterxml.jackson.annotation.JsonProperty;

public class BookingRefundCompletedEvent {

    @JsonProperty("bookingId")
    private String bookingId;

    @JsonProperty("paymentId")
    private String paymentId;

    @JsonProperty("status")
    private String status;

    // Constructor, Getters & Setters

    public BookingRefundCompletedEvent() {
    }

    public BookingRefundCompletedEvent(String bookingId, String paymentId, String status) {
        this.bookingId = bookingId;
        this.paymentId = paymentId;
        this.status = status;
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

    public String getStatus() {
        return status;
    }

    public void setStatus(String status) {
        this.status = status;
    }
}
