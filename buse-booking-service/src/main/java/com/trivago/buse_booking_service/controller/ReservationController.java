package com.trivago.buse_booking_service.controller;

import com.trivago.buse_booking_service.model.Reservation;
import com.trivago.buse_booking_service.repository.ReservationRepository;
import com.trivago.buse_booking_service.messaging.ReservationEventPublisher;
import com.trivago.buse_booking_service.messaging.ReservationCreatedEvent;

import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.tags.Tag;
import io.swagger.v3.oas.annotations.responses.ApiResponse;
import io.swagger.v3.oas.annotations.media.Content;
import io.swagger.v3.oas.annotations.media.Schema;

import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/reservations")
@Tag(name = "reservation-controller", description = "API documentation for reservation-related operations")
public class ReservationController {

    private final ReservationRepository repository;
    private final ReservationEventPublisher publisher;

    public ReservationController(ReservationRepository repository, ReservationEventPublisher publisher) {
        this.repository = repository;
        this.publisher = publisher;
    }

    @GetMapping
    @Operation(summary = "Returns a list of all reservations",
               responses = {
                   @ApiResponse(responseCode = "200", description = "Successfully retrieved the list of reservations",
                                content = @Content(mediaType = "application/json",
                                schema = @Schema(implementation = Reservation.class)))
               })
    public List<Reservation> getAll() {
        return repository.findAll();
    }

    @PostMapping
    @Operation(summary = "Creates a new reservation",
               responses = {
                   @ApiResponse(responseCode = "200", description = "Reservation successfully created",
                                content = @Content(mediaType = "application/json",
                                schema = @Schema(implementation = Reservation.class)))
               })
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
