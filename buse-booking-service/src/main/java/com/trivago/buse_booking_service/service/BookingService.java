package com.trivago.buse_booking_service.service;

import com.trivago.buse_booking_service.model.Booking;
import com.trivago.buse_booking_service.model.BookingStatus;
import com.trivago.buse_booking_service.repository.BookingRepository;
import jakarta.transaction.Transactional;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

//import java.time.LocalDate;
import java.util.List;
import java.util.Optional;

@Service
public class BookingService {

    @Autowired
    private BookingRepository bookingRepository;

    public Booking createBooking(Booking booking) {
        // Oda aynı tarihte zaten rezerve edilmişse hata
        List<Booking> conflicts = bookingRepository
                .findByRoomIdAndCheckInDateLessThanEqualAndCheckOutDateGreaterThanEqual(
                        booking.getRoomId(), booking.getCheckOutDate(), booking.getCheckInDate());

        if (!conflicts.isEmpty()) {
            throw new IllegalStateException("This room is already booked for the selected dates.");
        }

        booking.setStatus(BookingStatus.PENDING); // Saga'dan sonra CONFIRMED olacak
        return bookingRepository.save(booking);
    }

    public Optional<Booking> getBooking(Long id) {
        return bookingRepository.findById(id);
    }

    public List<Booking> getBookingsByUser(String userId) {
        return bookingRepository.findByUserId(userId);
    }

    @Transactional
    public Booking cancelBooking(Long bookingId) {
        Booking booking = bookingRepository.findById(bookingId)
                .orElseThrow(() -> new RuntimeException("Booking not found"));

        booking.setStatus(BookingStatus.CANCELLED);
        return booking; // JPA transactional olduğu için otomatik update edilir
    }
}
