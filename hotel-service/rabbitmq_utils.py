import pika
import json
import os
import logging
from dotenv import load_dotenv
from pika.exceptions import AMQPConnectionError, ChannelClosedByBroker

# Load environment variables
load_dotenv()

# Set up logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

import time

def get_rabbitmq_connection(retries=10, delay=5):
    """Create and return a connection to RabbitMQ, with retries."""
    host = os.getenv("RABBITMQ_HOST", "rabbitmq")
    port = int(os.getenv("RABBITMQ_PORT", "5672"))
    username = os.getenv("RABBITMQ_USERNAME", "guest")
    password = os.getenv("RABBITMQ_PASSWORD", "guest")
    vhost = os.getenv("RABBITMQ_VHOST", "/")

    credentials = pika.PlainCredentials(username, password)
    parameters = pika.ConnectionParameters(
        host=host,
        port=port,
        virtual_host=vhost,
        credentials=credentials,
        heartbeat=600,
        blocked_connection_timeout=300
    )

    for attempt in range(1, retries + 1):
        try:
            connection = pika.BlockingConnection(parameters)
            logger.info(f"[{attempt}] Connected to RabbitMQ at {host}:{port}")
            return connection
        except AMQPConnectionError as e:
            logger.warning(f"[{attempt}] RabbitMQ connection failed: {e}")
            if attempt == retries:
                logger.error("All connection attempts to RabbitMQ failed.")
                raise
            time.sleep(delay)

    """Create and return a connection to RabbitMQ."""
    try:
        host = os.getenv("RABBITMQ_HOST", "rabbitmq")
        port = int(os.getenv('RABBITMQ_PORT', '5672'))
        username = os.getenv('RABBITMQ_USERNAME', 'guest')
        password = os.getenv('RABBITMQ_PASSWORD', 'guest')
        vhost = os.getenv('RABBITMQ_VHOST', '/')

        credentials = pika.PlainCredentials(username, password)
        parameters = pika.ConnectionParameters(
            host=host,
            port=port,
            virtual_host=vhost,
            credentials=credentials,
            heartbeat=600,
            blocked_connection_timeout=300
        )

        connection = pika.BlockingConnection(parameters)
        logger.info(f"Successfully connected to RabbitMQ at {host}:{port}")
        return connection
    except AMQPConnectionError as e:
        logger.error(f"Failed to connect to RabbitMQ: {e}")
        raise

def publish_message(queue_name, message, exchange='', persistent=True):
    connection = None
    try:
        connection = get_rabbitmq_connection()
        channel = connection.channel()
        channel.queue_declare(queue=queue_name, durable=True)

        properties = pika.BasicProperties(
            delivery_mode=2 if persistent else 1,
            content_type='application/json'
        )

        channel.basic_publish(
            exchange=exchange,
            routing_key=queue_name,
            body=json.dumps(message),
            properties=properties
        )

        logger.info(f"Message published to {queue_name}: {message}")
        return True

    except (AMQPConnectionError, ChannelClosedByBroker) as e:
        logger.error(f"RabbitMQ error when publishing to {queue_name}: {e}")
        return False
    except Exception as e:
        logger.error(f"Unexpected error publishing to {queue_name}: {e}")
        return False
    finally:
        if connection and connection.is_open:
            connection.close()

def consume_messages(queue_name, callback_function):
    connection = None  # 🔧 Kritik düzeltme: önce tanımla
    try:
        connection = get_rabbitmq_connection()
        channel = connection.channel()

        channel.queue_declare(queue=queue_name, durable=True)
        channel.basic_qos(prefetch_count=1)

        channel.basic_consume(
            queue=queue_name,
            on_message_callback=callback_function,
            auto_ack=False
        )

        logger.info(f"Started consuming from {queue_name}")
        channel.start_consuming()

    except KeyboardInterrupt:
        logger.info("Consumer stopped by user")
    except Exception as e:
        logger.error(f"Error in consumer: {e}")
    finally:
        if connection and connection.is_open:
            connection.close()
            logger.info("RabbitMQ connection closed.")

def publish_event(queue_name, payload):
    connection = None
    try:
        connection = get_rabbitmq_connection()
        channel = connection.channel()
        channel.queue_declare(queue=queue_name, durable=True,auto_delete=False)

        channel.basic_publish(
            exchange='',
            routing_key=queue_name,
            body=json.dumps(payload),
            properties=pika.BasicProperties(
                delivery_mode=2,
                content_type='application/json'
            )
        )
        logger.info(f"Event published to {queue_name}: {payload}")
        return True
    except Exception as e:
        logger.error(f"Failed to publish event to {queue_name}: {e}")
        return False
    finally:
        if connection and connection.is_open:
            connection.close()
