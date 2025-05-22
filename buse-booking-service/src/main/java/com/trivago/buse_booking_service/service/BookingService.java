//service/BookingService.java

package com.trivago.buse_booking_service.service;

import com.trivago.buse_booking_service.messaging.booking_to_payment.BookingCreatedEvent;
import com.trivago.buse_booking_service.messaging.booking_to_payment.producer.BookingEventProducer;
import com.trivago.buse_booking_service.model.Booking;
import com.trivago.buse_booking_service.model.BookingStatus;
import com.trivago.buse_booking_service.repository.BookingRepository;
import jakarta.transaction.Transactional;
import org.springframework.stereotype.Service;

import java.time.LocalDate;
import java.util.*;
import java.util.stream.Collectors;

@Service
public class BookingService {

    private final BookingRepository bookingRepository;
    private final BookingEventProducer eventProducer;
    private final BookingHistoryService bookingHistoryService;

    public BookingService(BookingRepository bookingRepository,
            BookingEventProducer eventProducer,
            BookingHistoryService bookingHistoryService) {
        this.bookingRepository = bookingRepository;
        this.eventProducer = eventProducer;
        this.bookingHistoryService = bookingHistoryService;
    }

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
        Booking savedBooking = bookingRepository.save(booking);

        BookingCreatedEvent event = new BookingCreatedEvent(
                savedBooking.getBookingId().toString(),
                savedBooking.getUserId(),
                savedBooking.getAmount(),
                savedBooking.getCurrency());

        eventProducer.sendBookingCreatedEvent(event);
        bookingHistoryService.logHistory(savedBooking.getBookingId(), "CREATED");

        return savedBooking;
    }

    public Optional<Booking> getBooking(Long id) {
        return bookingRepository.findById(id);
    }

    public List<Booking> getBookingsByUser(String userId) {
        return bookingRepository.findByUserId(userId);
    }

    @Transactional
    public Booking cancelBooking(Long bookingId, String reason) {
        Booking booking = bookingRepository.findById(bookingId)
                .orElseThrow(() -> new RuntimeException("Booking not found"));
        booking.setStatus(BookingStatus.CANCELLED);
        return bookingRepository.save(booking);
    }

    public Map<String, Long> getBookingStatistics() {
        return bookingRepository.findAll().stream()
                .collect(Collectors.groupingBy(b -> b.getStatus().name(), Collectors.counting()));
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

    public void markAsPaid(Long bookingId, Long paymentId) {
        Booking booking = bookingRepository.findById(bookingId)
                .orElseThrow(() -> new RuntimeException("Booking not found"));
        booking.setStatus(BookingStatus.PAID);
        bookingRepository.save(booking);
    }
}
