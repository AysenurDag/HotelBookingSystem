package com.trivago.buse_booking_service.messaging;

import org.springframework.amqp.rabbit.core.RabbitTemplate;
import org.springframework.stereotype.Component;

import static com.trivago.buse_booking_service.config.RabbitMQConfig.EXCHANGE;

/**
 * Booking Service tarafından oluşturulan event'leri RabbitMQ'ya gönderir.
 */
@Component
public class BookingEventProducer {

    private final RabbitTemplate rabbitTemplate;

    public BookingEventProducer(RabbitTemplate rabbitTemplate) {
        this.rabbitTemplate = rabbitTemplate;
    }

    // ▶️ Hotel Service'e oda rezervasyonu bilgisi gönder
    public void sendCreatedEvent(ReservationCreatedEvent event) {
        rabbitTemplate.convertAndSend(EXCHANGE, "booking.reservation.created", event);
    }

    // ❌ Kullanıcı rezervasyonu iptal ettiğinde
    public void sendCancelledEvent(ReservationCancelledEvent event) {
        System.out.println("[📤] Sending Cancelled Event: " + event.getBookingId());
        rabbitTemplate.convertAndSend(EXCHANGE, "booking.cancelled", event);
    }

    public void sendBookingCreatedEvent(BookingCreatedEvent event) {
        System.out.println("[📤] Sending Booking Created Event: " + event.getBookingId());
        System.out.println("[📤] To Exchange: " + EXCHANGE);
        System.out.println("[📤] With Routing Key: booking.created");
        rabbitTemplate.convertAndSend(EXCHANGE, "booking.created", event);
    }

    // ✅ Her şey başarılıysa confirmation bilgisi gönder
    public void sendReservationConfirmedEvent(ReservationConfirmedEvent event) {
        rabbitTemplate.convertAndSend(EXCHANGE, "booking.confirmed", event);
    }
}
