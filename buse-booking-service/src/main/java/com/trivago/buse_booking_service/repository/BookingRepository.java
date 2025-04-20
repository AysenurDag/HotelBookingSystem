package com.trivago.buse_booking_service.repository;

import com.trivago.buse_booking_service.model.Booking;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.time.LocalDate;
import java.util.List;

@Repository
public interface BookingRepository extends JpaRepository<Booking, Long> {

    @SuppressWarnings("null")
    List<Booking> findAll();

    List<Booking> findByStatus(String status);

    List<Booking> findByUserId(String userId);

    List<Booking> findByCheckInDateBetweenOrCheckOutDateBetween(
            LocalDate start1, LocalDate end1, LocalDate start2, LocalDate end2);

    List<Booking> findByRoomIdAndCheckInDateLessThanEqualAndCheckOutDateGreaterThanEqual(
            String roomId,
            LocalDate checkout,
            LocalDate checkin);

    List<Booking> findByCheckInDateOrCheckOutDate(LocalDate checkIn, LocalDate checkOut);

}
