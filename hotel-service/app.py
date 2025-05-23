from factory import create_app
from database import mongo
from rabbitmq_utils import consume_messages
from consumer import handle_message
import threading
import os

def start_consumer_thread(app):
    from flask import current_app
    def start():
        with app.app_context():
            consume_messages("booking.reservation.created.queue", handle_message)
    threading.Thread(target=start, daemon=True).start()

if __name__ == '__main__':
    app = create_app(os.getenv('FLASK_ENV', 'development'))
    start_consumer_thread(app)
    app.run(host='0.0.0.0', port=5050, debug=app.config['DEBUG'])