package com.trivago.buse_booking_service.messaging;

/**
 * Tüm işlemler başarılı olduğunda yayınlanır.
 * Notification Service ve Frontend UI bu event'i kullanabilir.
 */
public class ReservationConfirmedEvent {

    private String bookingId;
    private String confirmationNumber;
    private String status;
    private String userId;

    public ReservationConfirmedEvent() {
    }

    public ReservationConfirmedEvent(String bookingId, String confirmationNumber, String status, String userId) {
        this.bookingId = bookingId;
        this.confirmationNumber = confirmationNumber;
        this.status = status;
        this.userId = userId;
    }

    public String getBookingId() {
        return bookingId;
    }

    public void setBookingId(String bookingId) {
        this.bookingId = bookingId;
    }

    public String getConfirmationNumber() {
        return confirmationNumber;
    }

    public void setConfirmationNumber(String confirmationNumber) {
        this.confirmationNumber = confirmationNumber;
    }

    public String getStatus() {
        return status;
    }

    public void setStatus(String status) {
        this.status = status;
    }

    public String getUserId() {
        return userId;
    }

    public void setUserId(String userId) {
        this.userId = userId;
    }
}
