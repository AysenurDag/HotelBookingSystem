package com.trivago.buse_booking_service.messaging.listener;

import com.trivago.buse_booking_service.messaging.PaymentSucceededEvent;
import com.trivago.buse_booking_service.messaging.PaymentFailedEvent;
import com.trivago.buse_booking_service.service.BookingService;
import org.springframework.amqp.rabbit.annotation.RabbitListener;
import org.springframework.stereotype.Component;

/**
 * Payment Service tarafından yayınlanan event'leri dinler:
 * - Ödeme başarılıysa booking'i PAID olarak işaretler
 * - Ödeme başarısızsa rezervasyonu CANCELLED yapar
 */
@Component
public class PaymentEventListener {

    private final BookingService bookingService;

    public PaymentEventListener(BookingService bookingService) {
        this.bookingService = bookingService;
    }

    // Payment başarılıysa çağrılır
    @RabbitListener(queues = "${rabbitmq.queue.paymentSucceeded}")
    public void handlePaymentSucceeded(PaymentSucceededEvent event) {
        bookingService.markAsPaid(event.getBookingId(), event.getPaymentId());
    }

    // Payment başarısızsa çağrılır
    @RabbitListener(queues = "${rabbitmq.queue.paymentFailed}")
    public void handlePaymentFailed(PaymentFailedEvent event) {
        bookingService.cancelBooking(event.getBookingId(), "PAYMENT_FAILED");
    }
}
