package com.trivago.buse_booking_service.service;

import com.trivago.buse_booking_service.model.BookingHistory;
import com.trivago.buse_booking_service.repository.BookingHistoryRepository;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import java.time.LocalDateTime;
import java.util.List;

@Service
public class BookingHistoryService {

    @Autowired
    private BookingHistoryRepository bookingHistoryRepository;

    public void logHistory(Long bookingId, String action) {
        BookingHistory history = new BookingHistory();
        history.setBookingId(bookingId);
        history.setAction(action);
        history.setTimestamp(LocalDateTime.now());

        bookingHistoryRepository.save(history);
    }

    public List<BookingHistory> getHistoryForBooking(Long bookingId) {
        return bookingHistoryRepository.findByBookingId(bookingId);
    }
}
