package com.trivago.buse_booking_service.messaging.payment_to_booking.listener;

import com.trivago.buse_booking_service.messaging.payment_to_booking.PaymentSucceededEvent;
import com.trivago.buse_booking_service.model.BookingStatus;
import com.trivago.buse_booking_service.model.Booking;
import com.trivago.buse_booking_service.repository.BookingRepository;
import com.trivago.buse_booking_service.messaging.booking_to_hotel.ReservationCreatedEvent;
import com.trivago.buse_booking_service.messaging.booking_to_hotel.producer.ReservationEventProducer;
import org.springframework.amqp.rabbit.annotation.RabbitListener;
import org.springframework.stereotype.Component;
import org.springframework.beans.factory.annotation.Autowired;

@Component
public class PaymentEventListener {

    @Autowired
    private BookingRepository bookingRepository;

    @Autowired
    private ReservationEventProducer reservationEventProducer;

    @RabbitListener(queues = "payment.success.queue")
    public void handlePaymentSuccess(PaymentSucceededEvent event) {
        Long bookingId = event.getBookingId();

        Booking booking = bookingRepository.findById(bookingId)
                .orElseThrow(() -> new RuntimeException("Booking not found: " + bookingId));

        booking.setStatus(BookingStatus.PAID);
        bookingRepository.save(booking);

        ReservationCreatedEvent reservationEvent = new ReservationCreatedEvent(
                booking.getBookingId(),
                booking.getRoomId(),
                booking.getUserId(),
                booking.getCheckInDate(),
                booking.getCheckOutDate()
        );
        reservationEventProducer.sendReservationCreatedEvent(reservationEvent);
    }
}
