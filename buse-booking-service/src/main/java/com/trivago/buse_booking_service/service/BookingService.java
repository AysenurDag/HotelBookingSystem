package com.trivago.buse_booking_service.service;

import com.trivago.buse_booking_service.messaging.BookingEventProducer;
import com.trivago.buse_booking_service.messaging.ReservationCancelledEvent;
import com.trivago.buse_booking_service.messaging.ReservationConfirmedEvent;
import com.trivago.buse_booking_service.messaging.ReservationCreatedEvent;
import com.trivago.buse_booking_service.model.Booking;
import com.trivago.buse_booking_service.model.BookingStatus;
import com.trivago.buse_booking_service.repository.BookingRepository;
import jakarta.transaction.Transactional;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import java.util.Map;
import java.util.stream.Collectors;

import java.time.LocalDate;
//import java.time.LocalDate;
import java.util.List;
import java.util.Optional;

@Service
public class BookingService {

    @Autowired
    private BookingRepository bookingRepository;

    @Autowired
    private BookingEventProducer eventProducer;

    public List<Booking> getAllBookings() {
        return bookingRepository.findAll();
    }

    public List<Booking> getBookingsByStatus(String status) {
        return bookingRepository.findByStatus(BookingStatus.valueOf(status.toUpperCase()));
    }

    public List<Booking> getBookingsInDateRange(LocalDate start, LocalDate end) {
        return bookingRepository.findByCheckInDateBetweenOrCheckOutDateBetween(start, end, start, end);
    }

    public Booking createBooking(Booking booking) {
        List<Booking> conflicts = bookingRepository
                .findByRoomIdAndCheckInDateLessThanEqualAndCheckOutDateGreaterThanEqual(
                        booking.getRoomId(), booking.getCheckOutDate(), booking.getCheckInDate());

        if (!conflicts.isEmpty()) {
            throw new IllegalStateException("This room is already booked for the selected dates.");
        }

        booking.setStatus(BookingStatus.PENDING); // Saga sonrası CONFIRMED olabilir
        Booking saved = bookingRepository.save(booking);

        // ▶️ Event gönder
        ReservationCreatedEvent event = new ReservationCreatedEvent(
                saved.getBookingId(),
                saved.getRoomId(),
                saved.getUserId(),
                saved.getCheckInDate(),
                saved.getCheckOutDate());
        eventProducer.sendCreatedEvent(event);

        return saved;
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

        // ▶️ Cancel event gönder
        ReservationCancelledEvent event = new ReservationCancelledEvent(
                booking.getBookingId(),
                "Cancelled by user or system");
        eventProducer.sendCancelledEvent(event);

        return booking;
    }

    public Map<String, Long> getBookingStatistics() {
        List<Booking> allBookings = bookingRepository.findAll();
        return allBookings.stream()
                .collect(Collectors.groupingBy(
                        booking -> booking.getStatus().name(),
                        Collectors.counting()));
    }

    public List<Booking> getBookingsToday() {
        LocalDate today = LocalDate.now();
        return bookingRepository.findByCheckInDateOrCheckOutDate(today, today);
    }

    public List<Booking> getTodaysBookings() {
        LocalDate today = LocalDate.now();
        return bookingRepository.findByCheckInDateOrCheckOutDate(today, today);
    }

    public void deleteBooking(Long id) {
        if (!bookingRepository.existsById(id)) {
            throw new IllegalArgumentException("Booking not found");
        }
        bookingRepository.deleteById(id);
    }

    public void markAsPaid(String bookingId, String paymentId) {
        Booking booking = bookingRepository.findById(Long.valueOf(bookingId))
                .orElseThrow(() -> new RuntimeException("Booking not found"));

        // Booking'i PAID olarak güncelle
        booking.setStatus(BookingStatus.PAID);
        // İsteğe bağlı olarak paymentId alanın varsa set edebilirsin

        bookingRepository.save(booking);

        // ▶️ BookingConfirmed event gönder
        eventProducer.sendReservationConfirmedEvent(new ReservationConfirmedEvent(
                bookingId,
                "CNF-" + bookingId, // örnek bir confirmationNumber
                "CONFIRMED",
                booking.getUserId()));
    }

    public void cancelBooking(String bookingId, String reason) {
        Booking booking = bookingRepository.findById(Long.valueOf(bookingId))
                .orElseThrow(() -> new RuntimeException("Booking not found"));

        booking.setStatus(BookingStatus.CANCELLED);
        bookingRepository.save(booking);

        // ▶️ ReservationCancelled event gönder
        eventProducer.sendCancelledEvent(new ReservationCancelledEvent(
                booking.getBookingId(),
                reason));
    }

}
