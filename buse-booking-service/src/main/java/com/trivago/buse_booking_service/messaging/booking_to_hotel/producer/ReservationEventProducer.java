package com.trivago.buse_booking_service.messaging.booking_to_hotel.producer;

import com.trivago.buse_booking_service.messaging.booking_to_hotel.ReservationCreatedEvent;
import com.trivago.buse_booking_service.messaging.booking_to_hotel.ReservationCancelledEvent;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.amqp.rabbit.core.RabbitTemplate;
import org.springframework.stereotype.Component;

@Component
public class ReservationEventProducer {

    private static final Logger log = LoggerFactory.getLogger(ReservationEventProducer.class);
    private final RabbitTemplate rabbitTemplate;

    private static final String RESERVATION_CREATED_QUEUE = "booking.reservation.created.queue";
    private static final String RESERVATION_CANCELLED_QUEUE = "booking.reservation.cancelled.queue";

    public ReservationEventProducer(RabbitTemplate rabbitTemplate) {
        this.rabbitTemplate = rabbitTemplate;
    }

    public void sendReservationCreatedEvent(ReservationCreatedEvent event) {
        rabbitTemplate.convertAndSend(RESERVATION_CREATED_QUEUE, event);
        log.info("📤 Sent ReservationCreatedEvent to queue [{}] for bookingId={}",
                RESERVATION_CREATED_QUEUE, event.getBookingId());
    }

    public void sendReservationCancelledEvent(ReservationCancelledEvent event) {
        rabbitTemplate.convertAndSend(RESERVATION_CANCELLED_QUEUE, event);
        log.info("📤 Sent ReservationCancelledEvent to queue [{}] for bookingId={}",
                RESERVATION_CANCELLED_QUEUE, event.getBookingId());
    }
}
