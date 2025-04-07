import pika
import os
from dotenv import load_dotenv
import logging

# Configure logging
logging.basicConfig(level=logging.INFO, 
                    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

# Load environment variables
load_dotenv()

def test_rabbitmq_connection():
    """Test connection to RabbitMQ server"""
    try:
        # Get connection parameters from environment variables with defaults
        host = os.getenv('RABBITMQ_HOST', '10.47.7.151')
        port = int(os.getenv('RABBITMQ_PORT', '5672'))  # Note: using 5672, not 15672
        username = os.getenv('RABBITMQ_USERNAME', 'guest')
        password = os.getenv('RABBITMQ_PASSWORD', 'guest')
        vhost = os.getenv('RABBITMQ_VHOST', '/')
        
        logger.info(f"Attempting to connect to RabbitMQ at {host}:{port}")
        
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
        
        # Test creating a channel
        channel = connection.channel()
        logger.info("Successfully created channel")
        
        # Close connection
        connection.close()
        logger.info("Connection closed successfully")
        return True
        
    except Exception as e:
        logger.error(f"Failed to connect to RabbitMQ: {str(e)}")
        return False

if __name__ == "__main__":
    print("Testing RabbitMQ connection...")
    result = test_rabbitmq_connection()
    if result:
        print("✅ Connection test successful!")
    else:
        print("❌ Connection test failed!")