from rabbitmq_utils import consume_messages
import json
import logging

logging.basicConfig(level=logging.INFO)

def handle_message(ch, method, properties, body):
    try:
        message = json.loads(body)
        logging.info(f"[Hotel Consumer] Message received: {message}")
        # Buraya mesajı işleyen kodu yazabilirsin (MongoDB'ye yazma, log, vs.)
        ch.basic_ack(delivery_tag=method.delivery_tag)
    except Exception as e:
        logging.error(f"[Hotel Consumer] Failed to process message: {e}")
        ch.basic_nack(delivery_tag=method.delivery_tag, requeue=False)

if __name__ == "__main__":
    consume_messages("booking.reservation.created.queue", handle_message)
