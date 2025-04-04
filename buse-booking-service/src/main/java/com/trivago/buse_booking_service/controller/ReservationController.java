package com.trivago.buse_booking_service.controller;

import com.trivago.buse_booking_service.model.Reservation;
import com.trivago.buse_booking_service.repository.ReservationRepository;
import com.trivago.buse_booking_service.messaging.ReservationEventPublisher;
import com.trivago.buse_booking_service.messaging.ReservationCreatedEvent;

import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/reservations")
public class ReservationController {

    private final ReservationRepository repository;
    private final ReservationEventPublisher publisher;

    public ReservationController(ReservationRepository repository, ReservationEventPublisher publisher) {
        this.repository = repository;
        this.publisher = publisher;
    }

    @GetMapping
    public List<Reservation> getAll() {
        return repository.findAll();
    }

    @PostMapping
    public Reservation create(@RequestBody Reservation reservation) {
        Reservation saved = repository.save(reservation);

        ReservationCreatedEvent event = new ReservationCreatedEvent(
                saved.getId(),
                saved.getUserId(),
                saved.getHotelId()
        );

        publisher.publishReservationCreated(event);

        return saved;
    }
}
