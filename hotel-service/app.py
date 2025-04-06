from flask import Flask
from api.routes import api_blueprint
import os
import logging
from config import config
from database import mongo, init_db

# Create a global mongo variable
mongo = None

def create_app(config_name='default'):
    app = Flask(__name__)
    app.config.from_object(config[config_name])
    
    # Setup logging
    logging.basicConfig(
        level=logging.INFO,
        format='%(asctime)s [%(levelname)s] - %(message)s'
    )
    
    # Initialize MongoDB
    init_db(app)

    # Register blueprints
    app.register_blueprint(api_blueprint, url_prefix='/api')
    
    return app

if __name__ == '__main__':
    app = create_app(os.getenv('FLASK_ENV', 'development'))
    app.run(host='0.0.0.0', port=5000, debug=app.config['DEBUG'])