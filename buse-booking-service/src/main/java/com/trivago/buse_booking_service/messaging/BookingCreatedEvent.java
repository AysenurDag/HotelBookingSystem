package com.trivago.buse_booking_service.messaging;

/**
 * Booking oluşturulduğunda gönderilir.
 * Payment Service bu event'i dinler ve ödeme sürecini başlatır.
 */
public class BookingCreatedEvent {

    private String bookingId;
    private String userId;
    private Double totalAmount;
    private String currency;

    public BookingCreatedEvent() {
    }

    public BookingCreatedEvent(String bookingId, String userId, Double totalAmount, String currency) {
        this.bookingId = bookingId;
        this.userId = userId;
        this.totalAmount = totalAmount;
        this.currency = currency;
    }

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

    public Double getTotalAmount() {
        return totalAmount;
    }

    public void setTotalAmount(Double totalAmount) {
        this.totalAmount = totalAmount;
    }

    public String getCurrency() {
        return currency;
    }

    public void setCurrency(String currency) {
        this.currency = currency;
    }
}
