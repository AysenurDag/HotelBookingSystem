package com.trivago.buse_booking_service.controller;

import com.trivago.buse_booking_service.messaging.booking_to_payment.BookingCancelledEvent;
import com.trivago.buse_booking_service.messaging.booking_to_payment.BookingCreatedEvent;
import com.trivago.buse_booking_service.messaging.booking_to_payment.producer.BookingEventProducer;
import com.trivago.buse_booking_service.model.Booking;
import com.trivago.buse_booking_service.service.BookingHistoryService;
import com.trivago.buse_booking_service.service.BookingService;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.tags.Tag;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.format.annotation.DateTimeFormat;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.time.LocalDate;
import java.util.List;
import java.util.Map;

@Tag(name = "Booking API", description = "Endpoints for hotel bookings")
@RestController
@RequestMapping("/api/bookings")
public class BookingController {

    @Autowired
    private BookingService bookingService;

    @Autowired
    private BookingHistoryService bookingHistoryService;

    @Autowired
    private BookingEventProducer bookingProducer;

    @PostMapping
    @Operation(summary = "Create a new booking")
    public ResponseEntity<Booking> createBooking(@RequestBody Booking booking) {
        Booking savedBooking = bookingService.createBooking(booking);
        bookingHistoryService.logHistory(savedBooking.getBookingId(), "CREATED");

        BookingCreatedEvent event = new BookingCreatedEvent(
                savedBooking.getBookingId().toString(),
                savedBooking.getUserId(),
                savedBooking.getAmount(),
                savedBooking.getCurrency());
        bookingProducer.sendBookingCreatedEvent(event);

        return ResponseEntity.ok(savedBooking);
    }

    @PostMapping("/{id}/cancel")
    @Operation(summary = "Cancel a booking")
    public ResponseEntity<Booking> cancelBooking(
            @PathVariable Long id,
            @RequestParam(name = "reason", required = false, defaultValue = "Cancelled by user") String reason) {

        Booking cancelled = bookingService.cancelBooking(id, reason);
        bookingHistoryService.logHistory(cancelled.getBookingId(), "CANCELLED: " + reason);

        BookingCancelledEvent event = new BookingCancelledEvent(
                cancelled.getBookingId().toString(),
                cancelled.getUserId(),
                cancelled.getAmount(),
                cancelled.getCurrency(),
                reason);
        bookingProducer.sendBookingCancelledEvent(event);

        return ResponseEntity.ok(cancelled);
    }

    @GetMapping("/{id}")
    @Operation(summary = "Get booking by ID")
    public ResponseEntity<Booking> getBooking(@PathVariable Long id) {
        return bookingService.getBooking(id)
                .map(ResponseEntity::ok)
                .orElse(ResponseEntity.notFound().build());
    }

    @GetMapping
    @Operation(summary = "Get all bookings")
    public ResponseEntity<List<Booking>> getAllBookings() {
        return ResponseEntity.ok(bookingService.getAllBookings());
    }

    @DeleteMapping("/{id}")
    @Operation(summary = "Delete a booking")
    public ResponseEntity<Void> deleteBooking(@PathVariable Long id) {
        bookingService.deleteBooking(id);
        return ResponseEntity.noContent().build();
    }

    @GetMapping("/user/{userId}")
    @Operation(summary = "Get bookings by user ID")
    public ResponseEntity<List<Booking>> getBookingsByUser(@PathVariable String userId) {
        return ResponseEntity.ok(bookingService.getBookingsByUser(userId));
    }

    @GetMapping("/date-range")
    @Operation(summary = "Get bookings within a date range")
    public ResponseEntity<List<Booking>> getBookingsByDateRange(
            @RequestParam("start") @DateTimeFormat(iso = DateTimeFormat.ISO.DATE) LocalDate start,
            @RequestParam("end") @DateTimeFormat(iso = DateTimeFormat.ISO.DATE) LocalDate end) {
        return ResponseEntity.ok(bookingService.getBookingsInDateRange(start, end));
    }

    @GetMapping("/status/{status}")
    @Operation(summary = "Get bookings by status")
    public ResponseEntity<List<Booking>> getBookingsByStatus(@PathVariable String status) {
        return ResponseEntity.ok(bookingService.getBookingsByStatus(status));
    }

    @GetMapping("/today")
    @Operation(summary = "Get bookings for today")
    public ResponseEntity<List<Booking>> getTodaysBookings() {
        return ResponseEntity.ok(bookingService.getTodaysBookings());
    }

    @GetMapping("/statistics")
    @Operation(summary = "Get booking statistics by status")
    public ResponseEntity<Map<String, Long>> getBookingStatistics() {
        return ResponseEntity.ok(bookingService.getBookingStatistics());
    }

    @GetMapping("/{id}/history")
    @Operation(summary = "Get booking history")
    public ResponseEntity<?> getBookingHistory(@PathVariable Long id) {
        return ResponseEntity.ok(bookingHistoryService.getHistoryForBooking(id));
    }
}
