package com.trivago.buse_booking_service.messaging;

import org.springframework.amqp.rabbit.core.RabbitTemplate;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

@Component
public class BookingEventProducer {

    private final RabbitTemplate rabbitTemplate;

    public BookingEventProducer(RabbitTemplate rabbitTemplate) {
        this.rabbitTemplate = rabbitTemplate;
    }

    @Value("${rabbitmq.exchange.name}")
    private String exchange;

    @Value("${rabbitmq.routing.created}")
    private String createdRoutingKey;

    @Value("${rabbitmq.routing.cancelled}")
    private String cancelledRoutingKey;

    public void sendCreatedEvent(ReservationCreatedEvent event) {
        rabbitTemplate.convertAndSend(exchange, createdRoutingKey, event);
    }

    public void sendCancelledEvent(ReservationCancelledEvent event) {
        rabbitTemplate.convertAndSend(exchange, cancelledRoutingKey, event);
    }
}
