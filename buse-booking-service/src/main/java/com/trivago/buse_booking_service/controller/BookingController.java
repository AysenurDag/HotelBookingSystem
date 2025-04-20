package com.trivago.buse_booking_service.controller;

import com.trivago.buse_booking_service.model.Booking;
import com.trivago.buse_booking_service.service.BookingHistoryService;
import com.trivago.buse_booking_service.service.BookingService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/bookings")
public class BookingController {

    @Autowired
    private BookingService bookingService;

    @Autowired
    private BookingHistoryService bookingHistoryService;

    // ▶️ 1. Rezervasyon oluştur
    @PostMapping
    public ResponseEntity<Booking> createBooking(@RequestBody Booking booking) {
        Booking savedBooking = bookingService.createBooking(booking);
        bookingHistoryService.logHistory(savedBooking.getBookingId(), "CREATED");
        return ResponseEntity.ok(savedBooking);
    }

    // ▶️ 2. Belirli bir rezervasyonu getir
    @GetMapping("/{id}")
    public ResponseEntity<Booking> getBooking(@PathVariable Long id) {
        return bookingService.getBooking(id)
                .map(ResponseEntity::ok)
                .orElse(ResponseEntity.notFound().build());
    }

    // ▶️ 3. Kullanıcının rezervasyonlarını getir
    @GetMapping("/user/{userId}")
    public ResponseEntity<List<Booking>> getBookingsByUser(@PathVariable String userId) {
        return ResponseEntity.ok(bookingService.getBookingsByUser(userId));
    }

    // ▶️ 4. Rezervasyonu iptal et
    @PostMapping("/{id}/cancel")
    public ResponseEntity<Booking> cancelBooking(@PathVariable Long id) {
        Booking cancelled = bookingService.cancelBooking(id);
        bookingHistoryService.logHistory(cancelled.getBookingId(), "CANCELLED");
        return ResponseEntity.ok(cancelled);
    }

    // ▶️ 5. Rezervasyon geçmişini getir
    @GetMapping("/{id}/history")
    public ResponseEntity<?> getBookingHistory(@PathVariable Long id) {
        return ResponseEntity.ok(bookingHistoryService.getHistoryForBooking(id));
    }
}
