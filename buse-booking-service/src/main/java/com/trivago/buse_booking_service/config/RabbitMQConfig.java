package com.trivago.buse_booking_service.config;

import org.springframework.amqp.support.converter.Jackson2JsonMessageConverter;
import org.springframework.amqp.support.converter.MessageConverter;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

/**
 * Configuration class for RabbitMQ message conversion.
 * Registers a MessageConverter bean that automatically converts messages to/from JSON
 * using Jackson, allowing objects to be serialized and deserialized when sent via RabbitMQ.
 */
@Configuration
public class RabbitMQConfig {

    /**
     * Configures a message converter to use Jackson for JSON serialization.
     * This allows message payloads to be sent and received as JSON.
     *
     * @return the Jackson2JsonMessageConverter bean
     */
    @Bean
    public MessageConverter jsonMessageConverter() {
        return new Jackson2JsonMessageConverter();
    }
}
