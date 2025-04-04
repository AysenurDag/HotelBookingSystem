package com.trivago.buse_booking_service.messaging;

import org.springframework.amqp.rabbit.core.RabbitTemplate;
import org.springframework.stereotype.Service;

@Service
public class ReservationEventPublisher {

    private final RabbitTemplate rabbitTemplate;

    public ReservationEventPublisher(RabbitTemplate rabbitTemplate) {
        this.rabbitTemplate = rabbitTemplate;
    }

    public void publishReservationCreated(ReservationCreatedEvent event) {
        rabbitTemplate.convertAndSend("reservationQueue", event);
        System.out.println("ReservationCreatedEvent gönderildi: " + event.getReservationId());
    }
}
