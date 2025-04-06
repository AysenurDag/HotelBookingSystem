import pika
import json
import os
from dotenv import load_dotenv

load_dotenv()

def publish_message(queue_name, message):
    try:
        connection = pika.BlockingConnection(
            pika.ConnectionParameters(os.getenv('RABBITMQ_HOST', 'localhost'))
        )
        channel = connection.channel()
        channel.queue_declare(queue=queue_name)
        channel.basic_publish(
            exchange='',
            routing_key=queue_name,
            body=json.dumps(message)
        )
        connection.close()
        return True
    except Exception as e:
        print(f"Error publishing message: {e}")
        return False