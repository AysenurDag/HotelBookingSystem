package com.trivago.buse_booking_service.repository;

import com.trivago.buse_booking_service.model.Reservation;
import org.springframework.data.jpa.repository.JpaRepository;

public interface ReservationRepository extends JpaRepository<Reservation, Long> {
}
