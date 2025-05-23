package com.trivago.buse_booking_service;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;

/**
 * Main entry point for the Booking Service Spring Boot application.
 * This service handles reservation creation and publishes events to other services via RabbitMQ.
 *
 * <p>Usage: Run this class to start the Booking microservice independently.</p>
 */
@SpringBootApplication
public class BuseBookingServiceApplication {

    /**
     * Bootstraps the Booking Service application.
     *
     * @param args command-line arguments (not used)
     */
    public static void main(String[] args) {
        SpringApplication.run(BuseBookingServiceApplication.class, args);
    }
}
