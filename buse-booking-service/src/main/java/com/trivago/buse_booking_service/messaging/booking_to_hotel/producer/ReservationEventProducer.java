package com.trivago.buse_booking_service.messaging.booking_to_hotel.producer;

import com.trivago.buse_booking_service.messaging.booking_to_hotel.ReservationCreatedEvent;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.amqp.rabbit.core.RabbitTemplate;
import org.springframework.stereotype.Component;

@Component
public class ReservationEventProducer {
    private static final Logger log = LoggerFactory.getLogger(ReservationEventProducer.class);
    private final RabbitTemplate rabbitTemplate;
    private static final String QUEUE_NAME = "booking.reservation.created.queue";

    public ReservationEventProducer(RabbitTemplate rabbitTemplate) {
        this.rabbitTemplate = rabbitTemplate;
    }

    public void sendReservationCreatedEvent(ReservationCreatedEvent event) {
        rabbitTemplate.convertAndSend(QUEUE_NAME, event);
        log.info("📤 Sent ReservationCreatedEvent to queue [{}] for bookingId={}", QUEUE_NAME, event.getBookingId());
    }
}
