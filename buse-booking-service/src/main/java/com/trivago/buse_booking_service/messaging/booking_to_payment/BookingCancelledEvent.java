package com.trivago.buse_booking_service.messaging.booking_to_payment;

public class BookingCancelledEvent {
    private String bookingId;
    private String userId;
    private Double amount;
    private String currency;
    private String reason;

    public BookingCancelledEvent() {
    }

    public BookingCancelledEvent(String bookingId, String userId, Double amount, String currency, String reason) {
        this.bookingId = bookingId;
        this.userId = userId;
        this.amount = amount;
        this.currency = currency;
        this.reason = reason;
    }

    // Getters and Setters
    public String getBookingId() {
        return bookingId;
    }

    public void setBookingId(String bookingId) {
        this.bookingId = bookingId;
    }

    public String getUserId() {
        return userId;
    }

    public void setUserId(String userId) {
        this.userId = userId;
    }

    public Double getAmount() {
        return amount;
    }

    public void setAmount(Double amount) {
        this.amount = amount;
    }

    public String getCurrency() {
        return currency;
    }

    public void setCurrency(String currency) {
        this.currency = currency;
    }

    public String getReason() {
        return reason;
    }

    public void setReason(String reason) {
        this.reason = reason;
    }
}
