package com.trivago.buse_booking_service.model;

/**
 * Enum representing the various statuses a booking can have
 * throughout its lifecycle in the booking system.
 */
public enum BookingStatus {

    /**
     * Booking created but payment not yet processed.
     */
    PENDING,

    /**
     * Payment has been completed successfully.
     */
    PAID,

    /**
     * Payment attempt has failed.
     */
    PAYMENT_FAILED,

    /**
     * Booking has been cancelled either by user or system.
     */
    CANCELLED,

    /**
     * Payment was successful but later refunded.
     */
    REFUND
}
