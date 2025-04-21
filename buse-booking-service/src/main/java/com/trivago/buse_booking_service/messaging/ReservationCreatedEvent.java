package com.trivago.buse_booking_service.messaging;

import java.time.LocalDate;

/**
 * Hotel Service'e oda rezervasyonu için gönderilir.
 * Booking başarıyla kaydedildikten sonra yayınlanır.
 */
public class ReservationCreatedEvent {

    private Long bookingId;
    private String roomId;
    private String userId;
    private LocalDate checkInDate;
    private LocalDate checkOutDate;

    public ReservationCreatedEvent() {
    }

    public ReservationCreatedEvent(Long bookingId, String roomId, String userId,
            LocalDate checkInDate, LocalDate checkOutDate) {
        this.bookingId = bookingId;
        this.roomId = roomId;
        this.userId = userId;
        this.checkInDate = checkInDate;
        this.checkOutDate = checkOutDate;
    }

    public Long getBookingId() {
        return bookingId;
    }

    public void setBookingId(Long bookingId) {
        this.bookingId = bookingId;
    }

    public String getRoomId() {
        return roomId;
    }

    public void setRoomId(String roomId) {
        this.roomId = roomId;
    }

    public String getUserId() {
        return userId;
    }

    public void setUserId(String userId) {
        this.userId = userId;
    }

    public LocalDate getCheckInDate() {
        return checkInDate;
    }

    public void setCheckInDate(LocalDate checkInDate) {
        this.checkInDate = checkInDate;
    }

    public LocalDate getCheckOutDate() {
        return checkOutDate;
    }

    public void setCheckOutDate(LocalDate checkOutDate) {
        this.checkOutDate = checkOutDate;
    }
}
