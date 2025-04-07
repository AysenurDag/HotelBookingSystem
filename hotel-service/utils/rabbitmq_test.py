import time
import json
import sys
from rabbitmq_utils import publish_message, consume_messages

# Test queue name
TEST_QUEUE = "test_queue"

def message_callback(channel, method, properties, body):
    """Callback function for consumed messages"""
    try:
        message = json.loads(body)
        print(f"Received message: {message}")
        
        # Acknowledge the message
        channel.basic_ack(delivery_tag=method.delivery_tag)
    except Exception as e:
        print(f"Error processing message: {e}")
        # Reject the message and requeue
        channel.basic_nack(delivery_tag=method.delivery_tag, requeue=True)

def publish_test_message():
    """Publish a test message to RabbitMQ"""
    test_message = {
        "id": int(time.time()),
        "message": "This is a test message",
        "timestamp": time.strftime("%Y-%m-%d %H:%M:%S")
    }
    
    success = publish_message(TEST_QUEUE, test_message)
    if success:
        print(f"Successfully published test message: {test_message}")
    else:
        print("Failed to publish test message")

def start_consumer():
    """Start consuming messages from the test queue"""
    print(f"Starting consumer for queue: {TEST_QUEUE}")
    print("Press CTRL+C to exit")
    consume_messages(TEST_QUEUE, message_callback)

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python test_rabbitmq.py [publish|consume]")
        sys.exit(1)
        
    command = sys.argv[1]
    
    if command == "publish":
        publish_test_message()
    elif command == "consume":
        start_consumer()
    else:
        print("Unknown command. Use 'publish' or 'consume'")
        sys.exit(1)