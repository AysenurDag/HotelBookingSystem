package com.trivago.buse_booking_service.messaging.payment_to_booking.listener;

import com.trivago.buse_booking_service.messaging.payment_to_booking.BookingRefundCompletedEvent;
import com.trivago.buse_booking_service.model.Booking;
import com.trivago.buse_booking_service.model.BookingStatus;
import com.trivago.buse_booking_service.repository.BookingRepository;
import com.trivago.buse_booking_service.messaging.booking_to_hotel.ReservationCancelledEvent;
import com.trivago.buse_booking_service.messaging.booking_to_hotel.producer.ReservationEventProducer;
import org.springframework.amqp.rabbit.annotation.RabbitListener;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

@Component
public class RefundEventListener {

    @Autowired
    private BookingRepository bookingRepository;

    @Autowired
    private ReservationEventProducer reservationEventProducer;

    @RabbitListener(queues = "booking.refund.completed.queue")
    public void handleRefundCompletedEvent(BookingRefundCompletedEvent event) {
        Long bookingId = Long.parseLong(event.getBookingId());

        Booking booking = bookingRepository.findById(bookingId).orElse(null);
        if (booking == null) {
            System.err.println("❌ Booking not found for ID: " + bookingId);
            return;
        }

        booking.setStatus(BookingStatus.REFUND);
        bookingRepository.save(booking);
        System.out.println("✅ Booking marked as REFUND for bookingId=" + bookingId);

        ReservationCancelledEvent cancelledEvent = new ReservationCancelledEvent(
                booking.getBookingId(), booking.getRoomId(), booking.getUserId(),
                booking.getCheckInDate(), booking.getCheckOutDate());
        reservationEventProducer.sendReservationCancelledEvent(cancelledEvent);
    }
}
