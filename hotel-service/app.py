from factory import create_app
from database import mongo
from rabbitmq_utils import consume_messages
from consumer import handle_message, handle_cancellation_message 
import threading
import os

def start_consumer_thread(app, queue_name, callback):
    def start():
        with app.app_context():
            consume_messages(queue_name, callback)
    threading.Thread(target=start, daemon=True).start()

if __name__ == '__main__':
    app = create_app(os.getenv('FLASK_ENV', 'development'))

    # Booking reservation oluşturma mesajlarını dinle
    start_consumer_thread(app, "booking.reservation.created.queue", handle_message)

    # Reservation cancellation mesajlarını dinle
    start_consumer_thread(app, "booking.reservation.cancelled.queue", handle_cancellation_message)

    app.run(host='0.0.0.0', port=5050, debug=app.config['DEBUG'])
