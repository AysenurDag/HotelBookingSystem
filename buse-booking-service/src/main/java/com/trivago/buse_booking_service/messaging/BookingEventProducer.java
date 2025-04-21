package com.trivago.buse_booking_service.messaging;

import org.springframework.amqp.rabbit.core.RabbitTemplate;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

/**
 * Booking Service tarafından oluşturulan event'leri RabbitMQ'ya gönderir.
 */
@Component
public class BookingEventProducer {

    private final RabbitTemplate rabbitTemplate;

    public BookingEventProducer(RabbitTemplate rabbitTemplate) {
        this.rabbitTemplate = rabbitTemplate;
    }

    // Exchange adı (common)
    @Value("${rabbitmq.exchange.name}")
    private String exchange;

    // Routing key'ler
    @Value("${rabbitmq.routing.reservationCreated}")
    private String reservationCreatedRoutingKey;

    @Value("${rabbitmq.routing.cancelled}")
    private String cancelledRoutingKey;

    @Value("${rabbitmq.routing.bookingCreated}")
    private String bookingCreatedRoutingKey;

    @Value("${rabbitmq.routing.confirmed}")
    private String confirmedRoutingKey;

    // ▶️ Hotel Service'e oda rezervasyonu bilgisi gönder
    public void sendCreatedEvent(ReservationCreatedEvent event) {
        rabbitTemplate.convertAndSend(exchange, reservationCreatedRoutingKey, event);
    }

    // ❌ Saga başarısızsa cancel bilgisi gönder
    public void sendCancelledEvent(ReservationCancelledEvent event) {
        rabbitTemplate.convertAndSend(exchange, cancelledRoutingKey, event);
    }

    // 💳 Payment Service'e ödeme başlatılması için booking bilgisi gönder
    public void sendBookingCreatedEvent(BookingCreatedEvent event) {
        rabbitTemplate.convertAndSend(exchange, bookingCreatedRoutingKey, event);
    }

    // ✅ Her şey başarılıysa confirmation bilgisi gönder
    public void sendReservationConfirmedEvent(ReservationConfirmedEvent event) {
        rabbitTemplate.convertAndSend(exchange, confirmedRoutingKey, event);
    }
}
