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

def get_rabbitmq_connection():
    """Create and return a connection to RabbitMQ."""
    try:
        # Get connection parameters from environment variables with defaults
        host = os.getenv('RABBITMQ_HOST', 'localhost')
        port = int(os.getenv('RABBITMQ_PORT', '15672'))
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
    """
    Publish a message to a RabbitMQ queue.
    
    Args:
        queue_name (str): Name of the queue to publish to
        message (dict): Message to publish (will be JSON serialized)
        exchange (str): Exchange to use (empty string for default)
        persistent (bool): Whether to make the message persistent
        
    Returns:
        bool: True if successful, False otherwise
    """
    connection = None
    try:
        connection = get_rabbitmq_connection()
        channel = connection.channel()
        
        # Declare queue (creates if doesn't exist)
        channel.queue_declare(queue=queue_name, durable=True)
        
        # Set message properties
        properties = pika.BasicProperties(
            delivery_mode=2 if persistent else 1,  # 2 makes message persistent
            content_type='application/json'
        )
        
        # Publish message
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
        # Always close the connection
        if connection and connection.is_open:
            connection.close()

def consume_messages(queue_name, callback_function):
    """
    Set up a consumer for a RabbitMQ queue.
    
    Args:
        queue_name (str): Queue to consume from
        callback_function (callable): Function to call when message is received
                                     Should accept channel, method, properties, body
    """
    try:
        connection = get_rabbitmq_connection()
        channel = connection.channel()
        
        # Declare queue (creates if doesn't exist)
        channel.queue_declare(queue=queue_name, durable=True)
        
        # Set prefetch count to 1 to distribute tasks evenly
        channel.basic_qos(prefetch_count=1)
        
        # Set up consumer
        channel.basic_consume(
            queue=queue_name,
            on_message_callback=callback_function,
            auto_ack=False  # Don't auto-acknowledge
        )
        
        logger.info(f"Started consuming from {queue_name}")
        
        # Start consuming (blocks)
        channel.start_consuming()
        
    except KeyboardInterrupt:
        logger.info("Consumer stopped by user")
        if connection and connection.is_open:
            connection.close()
    except Exception as e:
        logger.error(f"Error in consumer: {e}")
        if connection and connection.is_open:
            connection.close()