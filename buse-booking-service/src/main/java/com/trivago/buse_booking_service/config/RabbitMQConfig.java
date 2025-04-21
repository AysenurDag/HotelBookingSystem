package com.trivago.buse_booking_service.config;

import org.springframework.amqp.core.*;
import org.springframework.amqp.rabbit.connection.ConnectionFactory;
import org.springframework.amqp.rabbit.core.RabbitTemplate;
import org.springframework.amqp.support.converter.Jackson2JsonMessageConverter;
import org.springframework.amqp.support.converter.MessageConverter;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

@Configuration
public class RabbitMQConfig {

    public static final String EXCHANGE = "booking.exchange";

    // Eventler ve kuyruğa bağlanacak routing key'ler
    public static final String BOOKING_CREATED_QUEUE = "booking.created.queue";
    public static final String PAYMENT_SUCCEEDED_QUEUE = "payment.success.queue";
    public static final String PAYMENT_FAILED_QUEUE = "payment.failed.queue";
    public static final String BOOKING_CANCELLED_QUEUE = "booking.cancelled.queue";
    public static final String RESERVATION_CREATED_QUEUE = "booking.reservation.created.queue";
    public static final String ROOM_FAILED_QUEUE = "hotel.room.failed.queue";
    public static final String RESERVATION_CONFIRMED_QUEUE = "booking.confirmed.queue";

    @Bean
    public TopicExchange exchange() {
        return new TopicExchange(EXCHANGE);
    }

    @Bean
    public Queue bookingCreatedQueue() {
        return new Queue(BOOKING_CREATED_QUEUE);
    }

    @Bean
    public Queue paymentSucceededQueue() {
        return new Queue(PAYMENT_SUCCEEDED_QUEUE);
    }

    @Bean
    public Queue paymentFailedQueue() {
        return new Queue(PAYMENT_FAILED_QUEUE);
    }

    @Bean
    public Queue bookingCancelledQueue() {
        return new Queue(BOOKING_CANCELLED_QUEUE);
    }

    @Bean
    public Queue reservationCreatedQueue() {
        return new Queue(RESERVATION_CREATED_QUEUE);
    }

    @Bean
    public Queue roomFailedQueue() {
        return new Queue(ROOM_FAILED_QUEUE);
    }

    @Bean
    public Queue reservationConfirmedQueue() {
        return new Queue(RESERVATION_CONFIRMED_QUEUE);
    }

    @Bean
    public Binding bindingBookingCreated() {
        return BindingBuilder.bind(bookingCreatedQueue()).to(exchange()).with("booking.created");
    }

    @Bean
    public Binding bindingPaymentSucceeded() {
        return BindingBuilder.bind(paymentSucceededQueue()).to(exchange()).with("payment.succeeded");
    }

    @Bean
    public Binding bindingPaymentFailed() {
        return BindingBuilder.bind(paymentFailedQueue()).to(exchange()).with("payment.failed");
    }

    @Bean
    public Binding bindingBookingCancelled() {
        return BindingBuilder.bind(bookingCancelledQueue()).to(exchange()).with("booking.cancelled");
    }

    @Bean
    public Binding bindingReservationCreated() {
        return BindingBuilder.bind(reservationCreatedQueue()).to(exchange()).with("booking.reservation.created");
    }

    @Bean
    public Binding bindingRoomFailed() {
        return BindingBuilder.bind(roomFailedQueue()).to(exchange()).with("hotel.room.failed");
    }

    @Bean
    public Binding bindingReservationConfirmed() {
        return BindingBuilder.bind(reservationConfirmedQueue()).to(exchange()).with("booking.confirmed");
    }

    // JSON mesaj desteği
    @Bean
    public MessageConverter jsonMessageConverter() {
        return new Jackson2JsonMessageConverter();
    }

    @Bean
    public RabbitTemplate rabbitTemplate(ConnectionFactory connectionFactory, MessageConverter messageConverter) {
        RabbitTemplate template = new RabbitTemplate(connectionFactory);
        template.setMessageConverter(messageConverter);
        return template;
    }
}
