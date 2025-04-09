package com.trivago.buse_booking_service.messaging;

/**
 * Event class representing the creation of a reservation.
 * This event is typically published to a message broker to notify other services
 * (e.g., Payment or Notification services) that a new reservation has been made.
 */
public class ReservationCreatedEvent {

    /**
     * Unique identifier of the reservation.
     */
    private Long reservationId;

    /**
     * Identifier of the user who made the reservation.
     */
    private String userId;

    /**
     * Identifier of the hotel where the reservation was made.
     */
    private String hotelId;

    /**
     * Default constructor for deserialization.
     */
    public ReservationCreatedEvent() {}

    /**
     * Constructs a ReservationCreatedEvent with given reservation details.
     *
     * @param reservationId the ID of the created reservation
     * @param userId        the ID of the user who made the reservation
     * @param hotelId       the ID of the hotel reserved
     */
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
