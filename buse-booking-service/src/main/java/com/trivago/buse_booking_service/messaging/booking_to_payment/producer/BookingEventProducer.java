package com.trivago.buse_booking_service.messaging.booking_to_payment.producer;

import com.trivago.buse_booking_service.messaging.booking_to_payment.BookingCreatedEvent;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.amqp.rabbit.core.RabbitTemplate;
import org.springframework.stereotype.Component;

import static com.trivago.buse_booking_service.config.RabbitMQConfig.EXCHANGE;

@Component
public class BookingEventProducer {
    private static final Logger log = LoggerFactory.getLogger(BookingEventProducer.class);
    private final RabbitTemplate rabbitTemplate;

    public BookingEventProducer(RabbitTemplate rabbitTemplate) {
        this.rabbitTemplate = rabbitTemplate;
    }

    public void sendBookingCreatedEvent(BookingCreatedEvent event) {
        log.info("📤 Sending BookingCreatedEvent: bookingId={} to routingKey=booking.created", event.getBookingId());
        rabbitTemplate.convertAndSend(EXCHANGE, "booking.created", event);
    }
}
