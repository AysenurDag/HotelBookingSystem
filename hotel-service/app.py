from flask import Flask
from api.routes import api_blueprint
import os
import logging
from config import config
from database import init_db
from flask_restx import Api

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
    
    # Create API with Swagger
    api = Api(
        api_blueprint,
        version='1.0',
        title='Hotel Service API',
        description='Hotel service swagger API',
        doc='/swagger'
    )
    
    # Setup all the API routes
    from api.routes import setup_routes
    setup_routes(api)
    
    # Register the blueprint
    app.register_blueprint(api_blueprint, url_prefix='/api')
    
    return app

if __name__ == '__main__':
    app = create_app(os.getenv('FLASK_ENV', 'development'))
    app.run(host='0.0.0.0', port=5000, debug=app.config['DEBUG'])