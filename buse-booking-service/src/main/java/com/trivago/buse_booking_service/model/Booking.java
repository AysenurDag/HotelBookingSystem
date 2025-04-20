package com.trivago.buse_booking_service.model;

import io.swagger.v3.oas.annotations.media.Schema;
import jakarta.persistence.*;
import java.time.LocalDate;

@Entity
@Schema(description = "Represents a hotel booking")
public class Booking {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Schema(description = "Unique booking ID", example = "1")
    private Long bookingId;

    @Schema(description = "ID of the booked room", example = "ROOM-101")
    private String roomId;

    @Schema(description = "User who made the booking", example = "USER-456")
    private String userId;

    @Schema(description = "Date of check-in", example = "2025-06-01")
    private LocalDate checkInDate;

    @Schema(description = "Date of check-out", example = "2025-06-05")
    private LocalDate checkOutDate;

    @Enumerated(EnumType.STRING)
    @Schema(description = "Current status of the booking", example = "PENDING")
    private BookingStatus status;

    // Getters and Setters
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

    public BookingStatus getStatus() {
        return status;
    }

    public void setStatus(BookingStatus status) {
        this.status = status;
    }
}
