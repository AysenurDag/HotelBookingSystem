package com.trivago.buse_booking_service.messaging.hotel_to_booking.listener;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.trivago.buse_booking_service.messaging.hotel_to_booking.ReservationApprovedEvent;
import com.trivago.buse_booking_service.model.Booking;
import com.trivago.buse_booking_service.model.BookingStatus;
import com.trivago.buse_booking_service.repository.BookingRepository;
import org.springframework.amqp.rabbit.annotation.RabbitListener;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

@Component
public class ReservationApprovedEventListener {

    @Autowired
    private ObjectMapper objectMapper;

    @Autowired
    private BookingRepository bookingRepository;

    @RabbitListener(queues = "reservation.approved.queue")
    public void handleReservationApprovedEvent(ReservationApprovedEvent event) {
        try {
            Booking booking = bookingRepository.findById(event.getBookingId()).orElse(null);
            if (booking != null) {
                booking.setStatus(BookingStatus.COMPLETED);
                bookingRepository.save(booking);
                System.out.println("✅ Booking marked as COMPLETED: bookingId=" + event.getBookingId());
            } else {
                System.err.println("❌ Booking not found for ID: " + event.getBookingId());
            }
        } catch (Exception e) {
            System.err.println("❌ Error processing ReservationApprovedEvent: " + e.getMessage());
        }
    }

}
