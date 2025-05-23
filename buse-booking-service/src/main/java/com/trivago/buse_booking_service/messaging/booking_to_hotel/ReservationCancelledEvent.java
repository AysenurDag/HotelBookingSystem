package com.trivago.buse_booking_service.messaging.booking_to_hotel;

import com.fasterxml.jackson.annotation.JsonFormat;
import com.fasterxml.jackson.annotation.JsonProperty;

import java.time.LocalDate;

public class ReservationCancelledEvent {

    @JsonProperty("bookingId")
    private Long bookingId;

    @JsonProperty("roomId")
    private String roomId;

    @JsonProperty("userId")
    private String userId;

    @JsonProperty("checkInDate")
    @JsonFormat(pattern = "yyyy-MM-dd")
    private LocalDate checkInDate;

    @JsonProperty("checkOutDate")
    @JsonFormat(pattern = "yyyy-MM-dd")
    private LocalDate checkOutDate;

    public ReservationCancelledEvent() {
    }

    public ReservationCancelledEvent(Long bookingId, String roomId, String userId,
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
