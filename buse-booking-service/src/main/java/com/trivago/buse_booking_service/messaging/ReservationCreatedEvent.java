package com.trivago.buse_booking_service.messaging;

public class ReservationCreatedEvent {
    private Long reservationId;
    private String userId;
    private String hotelId;

    public ReservationCreatedEvent() {}




    public ReservationCreatedEvent(Long reservationId, String userId, String hotelId) {
        this.reservationId = reservationId;
        this.userId = userId;
        this.hotelId = hotelId;
    }

    public Long getReservationId() {
        return reservationId;
    }

    public String getUserId() {
        return userId;
    }

    public String getHotelId() {
        return hotelId;
    }
}
