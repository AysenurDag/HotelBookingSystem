package com.trivago.buse_booking_service.controller;

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

    // ----------------------
    // 📌 CRUD işlemleri
    // ----------------------

    @Operation(summary = "Create a new booking")
    @PostMapping
    public ResponseEntity<Booking> createBooking(@RequestBody Booking booking) {
        // bookingId otomatik olarak veritabanı tarafından oluşturulacak
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

    @Operation(summary = "Get all bookings")
    @GetMapping
    public ResponseEntity<List<Booking>> getAllBookings() {
        return ResponseEntity.ok(bookingService.getAllBookings());
    }

    @Operation(summary = "Delete a booking")
    @DeleteMapping("/{id}")
    public ResponseEntity<Void> deleteBooking(@PathVariable Long id) {
        bookingService.deleteBooking(id);
        return ResponseEntity.noContent().build();
    }

    // ----------------------
    // 🔍 Filtreli aramalar
    // ----------------------

    @Operation(summary = "Get bookings by user ID")
    @GetMapping("/user/{userId}")
    public ResponseEntity<List<Booking>> getBookingsByUser(@PathVariable String userId) {
        return ResponseEntity.ok(bookingService.getBookingsByUser(userId));
    }

    @Operation(summary = "Get bookings within a date range")
    @GetMapping("/date-range")
    public ResponseEntity<List<Booking>> getBookingsByDateRange(
            @RequestParam("start") @DateTimeFormat(iso = DateTimeFormat.ISO.DATE) LocalDate start,
            @RequestParam("end") @DateTimeFormat(iso = DateTimeFormat.ISO.DATE) LocalDate end) {
        return ResponseEntity.ok(bookingService.getBookingsInDateRange(start, end));
    }

    @Operation(summary = "Get bookings by status")
    @GetMapping("/status/{status}")
    public ResponseEntity<List<Booking>> getBookingsByStatus(@PathVariable String status) {
        return ResponseEntity.ok(bookingService.getBookingsByStatus(status));
    }

    @Operation(summary = "Get bookings for today")
    @GetMapping("/today")
    public ResponseEntity<List<Booking>> getTodaysBookings() {
        return ResponseEntity.ok(bookingService.getTodaysBookings());
    }

    // ----------------------
    // 📊 İstatistikler
    // ----------------------

    @Operation(summary = "Get booking statistics by status")
    @GetMapping("/statistics")
    public ResponseEntity<Map<String, Long>> getBookingStatistics() {
        return ResponseEntity.ok(bookingService.getBookingStatistics());
    }

    // ----------------------
    // 🕓 İlgili kayıtlar
    // ----------------------

    @Operation(summary = "Get booking history")
    @GetMapping("/{id}/history")
    public ResponseEntity<?> getBookingHistory(@PathVariable Long id) {
        return ResponseEntity.ok(bookingHistoryService.getHistoryForBooking(id));
    }
}
