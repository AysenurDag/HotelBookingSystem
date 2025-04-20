package com.trivago.buse_booking_service.repository;

import com.trivago.buse_booking_service.model.BookingHistory;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;

@Repository
public interface BookingHistoryRepository extends JpaRepository<BookingHistory, Long> {

    List<BookingHistory> findByBookingId(Long bookingId);
}
