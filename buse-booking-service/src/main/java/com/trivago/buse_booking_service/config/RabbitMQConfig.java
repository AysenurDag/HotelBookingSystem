package com.trivago.buse_booking_service.config;

import org.springframework.amqp.core.*;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

@Configuration
public class RabbitMQConfig {

    public static final String EXCHANGE = "booking.exchange";
    public static final String CREATED_QUEUE = "booking.created.queue";
    public static final String CANCELLED_QUEUE = "booking.cancelled.queue";
    public static final String CREATED_ROUTING_KEY = "booking.created";
    public static final String CANCELLED_ROUTING_KEY = "booking.cancelled";

    @Bean
    public TopicExchange exchange() {
        return new TopicExchange(EXCHANGE);
    }

    @Bean
    public Queue createdQueue() {
        return new Queue(CREATED_QUEUE);
    }

    @Bean
    public Queue cancelledQueue() {
        return new Queue(CANCELLED_QUEUE);
    }

    @Bean
    public Binding bindingCreated() {
        return BindingBuilder
                .bind(createdQueue())
                .to(exchange())
                .with(CREATED_ROUTING_KEY);
    }

    @Bean
    public Binding bindingCancelled() {
        return BindingBuilder
                .bind(cancelledQueue())
                .to(exchange())
                .with(CANCELLED_ROUTING_KEY);
    }
}
