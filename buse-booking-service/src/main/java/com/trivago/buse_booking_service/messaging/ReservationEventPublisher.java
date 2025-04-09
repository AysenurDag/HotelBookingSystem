package com.trivago.buse_booking_service.messaging;

import org.springframework.amqp.rabbit.core.RabbitTemplate;
import org.springframework.stereotype.Service;

/**
 * Service responsible for publishing reservation-related events to the message broker.
 * Uses RabbitMQ to send messages to the specified queue.
 */
@Service
public class ReservationEventPublisher {

    private final RabbitTemplate rabbitTemplate;

    /**
     * Constructs the publisher with the given RabbitTemplate.
     *
     * @param rabbitTemplate the template used to send messages to RabbitMQ
     */
    public ReservationEventPublisher(RabbitTemplate rabbitTemplate) {
        this.rabbitTemplate = rabbitTemplate;
    }

    /**
     * Publishes a ReservationCreatedEvent to the "reservationQueue".
     * This method is used to notify other services that a new reservation has been created.
     *
     * @param event the event containing reservation details
     */
    public void publishReservationCreated(ReservationCreatedEvent event) {
        rabbitTemplate.convertAndSend("reservationQueue", event);
        System.out.println("ReservationCreatedEvent sent: " + event.getReservationId());
    }
}
