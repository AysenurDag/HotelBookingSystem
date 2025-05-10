package com.trivago.buse_booking_service.controller;

import com.trivago.buse_booking_service.messaging.BookingCreatedEvent;
import com.trivago.buse_booking_service.messaging.BookingEventProducer;
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

/**
 * BookingController exposes REST endpoints for managing hotel bookings.
 * This includes creating, cancelling, retrieving, and filtering bookings,
 * as well as booking history and statistics. It also emits events for
 * integration with the Payment microservice.
 */
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

    /**
     * Create a new booking and emit BookingCreatedEvent to PaymentService.
     *
     * @param booking Booking request body.
     * @return Saved Booking entity.
     */
    @Operation(summary = "Create a new booking", description = "Creates a new booking and emits a BookingCreatedEvent with amount and currency to PaymentService.")
    @PostMapping
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

    /**
     * Cancel an existing booking and mark it as CANCELLED.
     *
     * @param id Booking ID to cancel.
     * @return Cancelled Booking entity.
     */
    @Operation(summary = "Cancel a booking", description = "Marks the booking as CANCELLED and logs the change in booking history.")
    @PostMapping("/{id}/cancel")
    public ResponseEntity<Booking> cancelBooking(@PathVariable Long id) {
        Booking cancelled = bookingService.cancelBooking(id);
        bookingHistoryService.logHistory(cancelled.getBookingId(), "CANCELLED");
        return ResponseEntity.ok(cancelled);
    }

    /**
     * Retrieve a single booking by its ID.
     */
    @Operation(summary = "Get booking by ID", description = "Retrieves a specific booking by its unique ID.")
    @GetMapping("/{id}")
    public ResponseEntity<Booking> getBooking(@PathVariable Long id) {
        return bookingService.getBooking(id)
                .map(ResponseEntity::ok)
                .orElse(ResponseEntity.notFound().build());
    }

    /**
     * Retrieve all bookings in the system.
     */
    @Operation(summary = "Get all bookings", description = "Retrieves all bookings currently stored in the system.")
    @GetMapping
    public ResponseEntity<List<Booking>> getAllBookings() {
        return ResponseEntity.ok(bookingService.getAllBookings());
    }

    /**
     * Delete a booking by its ID.
     */
    @Operation(summary = "Delete a booking", description = "Deletes a booking permanently by its ID.")
    @DeleteMapping("/{id}")
    public ResponseEntity<Void> deleteBooking(@PathVariable Long id) {
        bookingService.deleteBooking(id);
        return ResponseEntity.noContent().build();
    }

    /**
     * Get all bookings made by a specific user.
     */
    @Operation(summary = "Get bookings by user ID", description = "Retrieves all bookings created by the specified user.")
    @GetMapping("/user/{userId}")
    public ResponseEntity<List<Booking>> getBookingsByUser(@PathVariable String userId) {
        return ResponseEntity.ok(bookingService.getBookingsByUser(userId));
    }

    /**
     * Get bookings made within a specific date range.
     */
    @Operation(summary = "Get bookings within a date range", description = "Retrieves bookings whose check-in dates fall between the given start and end dates.")
    @GetMapping("/date-range")
    public ResponseEntity<List<Booking>> getBookingsByDateRange(
            @RequestParam("start") @DateTimeFormat(iso = DateTimeFormat.ISO.DATE) LocalDate start,
            @RequestParam("end") @DateTimeFormat(iso = DateTimeFormat.ISO.DATE) LocalDate end) {
        return ResponseEntity.ok(bookingService.getBookingsInDateRange(start, end));
    }

    /**
     * Get all bookings filtered by their status.
     */
    @Operation(summary = "Get bookings by status", description = "Retrieves all bookings that match the given status (e.g. PENDING, PAID).")
    @GetMapping("/status/{status}")
    public ResponseEntity<List<Booking>> getBookingsByStatus(@PathVariable String status) {
        return ResponseEntity.ok(bookingService.getBookingsByStatus(status));
    }

    /**
     * Get all bookings for today's check-in or check-out.
     */
    @Operation(summary = "Get bookings for today", description = "Retrieves all bookings where check-in or check-out date is today.")
    @GetMapping("/today")
    public ResponseEntity<List<Booking>> getTodaysBookings() {
        return ResponseEntity.ok(bookingService.getTodaysBookings());
    }

    /**
     * Get booking statistics grouped by their status.
     */
    @Operation(summary = "Get booking statistics by status", description = "Returns the number of bookings grouped by status (e.g. how many are PAID, CANCELLED, etc).")
    @GetMapping("/statistics")
    public ResponseEntity<Map<String, Long>> getBookingStatistics() {
        return ResponseEntity.ok(bookingService.getBookingStatistics());
    }

    /**
     * Get history logs of a specific booking.
     */
    @Operation(summary = "Get booking history", description = "Retrieves history logs (status changes, actions) for a given booking.")
    @GetMapping("/{id}/history")
    public ResponseEntity<?> getBookingHistory(@PathVariable Long id) {
        return ResponseEntity.ok(bookingHistoryService.getHistoryForBooking(id));
    }

    /**
     * Temporary test controller to simulate BookingCreatedEvent.
     */
    @RestController
    @RequestMapping("/api/test")
    public class TestController {

        @Autowired
        private BookingEventProducer eventProducer;

        @PostMapping("/booking-event")
        public ResponseEntity<String> sendTestEvent() {
            BookingCreatedEvent event = new BookingCreatedEvent("42", "user-1", 999.0, "USD");
            eventProducer.sendBookingCreatedEvent(event);
            return ResponseEntity.ok("BookingCreatedEvent sent.");
        }
    }
}
