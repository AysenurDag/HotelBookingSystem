package com.trivago.buse_booking_service.model;

import io.swagger.v3.oas.annotations.media.Schema;
import jakarta.persistence.*;

import java.time.LocalDate;

@Entity
@Schema(description = "Represents a hotel reservation entity")
public class Reservation {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Schema(description = "Unique reservation ID", example = "1")
    private Long id;

    @Schema(description = "Unique identifier of the hotel", example = "HOTEL-123")
    private String hotelId;

    @Schema(description = "Identifier of the user who made the reservation", example = "USER-456")
    private String userId;

    @Schema(description = "Check-in date", example = "2025-06-01")
    private LocalDate checkInDate;

    @Schema(description = "Check-out date", example = "2025-06-05")
    private LocalDate checkOutDate;

    // Getters and Setters
    public Long getId() { return id; }
    public void setId(Long id) { this.id = id; }

    public String getHotelId() { return hotelId; }
    public void setHotelId(String hotelId) { this.hotelId = hotelId; }

    public String getUserId() { return userId; }
    public void setUserId(String userId) { this.userId = userId; }

    public LocalDate getCheckInDate() { return checkInDate; }
    public void setCheckInDate(LocalDate checkInDate) { this.checkInDate = checkInDate; }

    public LocalDate getCheckOutDate() { return checkOutDate; }
    public void setCheckOutDate(LocalDate checkOutDate) { this.checkOutDate = checkOutDate; }
}
