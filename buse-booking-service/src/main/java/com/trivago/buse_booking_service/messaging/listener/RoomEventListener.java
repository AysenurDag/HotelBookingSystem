package com.trivago.buse_booking_service.messaging.listener;

import com.trivago.buse_booking_service.messaging.RoomAvailabilityFailedEvent;
import com.trivago.buse_booking_service.service.BookingService;
import org.springframework.amqp.rabbit.annotation.RabbitListener;
import org.springframework.stereotype.Component;

/**
 * Hotel Service tarafından yayınlanan "oda uygun değil" event'ini dinler.
 * Bu event alındığında rezervasyon iptal edilir.
 */
@Component
public class RoomEventListener {

    private final BookingService bookingService;

    public RoomEventListener(BookingService bookingService) {
        this.bookingService = bookingService;
    }

    @RabbitListener(queues = "${rabbitmq.queue.roomFailed}")
    public void handleRoomAvailabilityFailed(RoomAvailabilityFailedEvent event) {
        bookingService.cancelBooking(event.getBookingId(), "ROOM_UNAVAILABLE");
    }
}
