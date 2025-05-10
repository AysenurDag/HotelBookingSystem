from flask import Flask
from api.routes import api_blueprint
import os
import logging
from config import config
from database import init_db
from flask_restx import Api
from flask_cors import CORS
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

def create_app(config_name='default'):
    app = Flask(__name__)
    CORS(app)
    app.config.from_object(config[config_name])
    # Setup logging
    logging.basicConfig(
        level=logging.INFO,
        format='%(asctime)s [%(levelname)s] - %(message)s'
    )
    
    # Initialize MongoDB
    init_db(app)
    
    # Create API with Swagger
    api = Api(
        api_blueprint,
        version='1.0',
        title='Hotel Service API',
        description='Hotel service swagger API',
        doc='/'
    )
    
    # Setup all the API routes
    from api.routes import setup_routes
    setup_routes(api)
    
    # Register the blueprint
    app.register_blueprint(api_blueprint)
    
    return app

if __name__ == '__main__':
    consume_messages("booking.reservation.created.queue", handle_message)
    app = create_app(os.getenv('FLASK_ENV', 'development'))
    app.run(host='0.0.0.0', port=5000, debug=app.config['DEBUG'])