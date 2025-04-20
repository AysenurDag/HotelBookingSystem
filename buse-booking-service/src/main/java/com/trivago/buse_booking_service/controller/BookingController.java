package com.trivago.buse_booking_service.controller;

import com.trivago.buse_booking_service.model.Booking;
import com.trivago.buse_booking_service.service.BookingHistoryService;
import com.trivago.buse_booking_service.service.BookingService;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.tags.Tag;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@Tag(name = "Booking API", description = "Endpoints for hotel bookings")
@RestController
@RequestMapping("/api/bookings")
public class BookingController {

    @Autowired
    private BookingService bookingService;

    @Autowired
    private BookingHistoryService bookingHistoryService;

    @Operation(summary = "Create a new booking")
    @PostMapping
    public ResponseEntity<Booking> createBooking(@RequestBody Booking booking) {
        Booking savedBooking = bookingService.createBooking(booking);
        bookingHistoryService.logHistory(savedBooking.getBookingId(), "CREATED");
        return ResponseEntity.ok(savedBooking);
    }
    @Operation(summary = "Cancel a booking")
    @PostMapping("/{id}/cancel")
    public ResponseEntity<Booking> cancelBooking(@PathVariable Long id) {
        Booking cancelled = bookingService.cancelBooking(id);
        bookingHistoryService.logHistory(cancelled.getBookingId(), "CANCELLED");
        return ResponseEntity.ok(cancelled);
    }


    @Operation(summary = "Get booking by ID")
    @GetMapping("/{id}")
    public ResponseEntity<Booking> getBooking(@PathVariable Long id) {
        return bookingService.getBooking(id)
                .map(ResponseEntity::ok)
                .orElse(ResponseEntity.notFound().build());
    }

    @GetMapping
@Operation(summary = "Get all bookings")
public ResponseEntity<List<Booking>> getAllBookings() {
    List<Booking> bookings = bookingService.getAllBookings();
    return ResponseEntity.ok(bookings);
}

    @Operation(summary = "Get bookings by user ID")
    @GetMapping("/user/{userId}")
    public ResponseEntity<List<Booking>> getBookingsByUser(@PathVariable String userId) {
        return ResponseEntity.ok(bookingService.getBookingsByUser(userId));
    }

  
    @Operation(summary = "Get booking history")
    @GetMapping("/{id}/history")
    public ResponseEntity<?> getBookingHistory(@PathVariable Long id) {
        return ResponseEntity.ok(bookingHistoryService.getHistoryForBooking(id));
    }
}
