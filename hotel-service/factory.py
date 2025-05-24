# hotel_service.py veya main.py
from flask import Flask
from config import config
from database import init_db
from flask_restx import Api
from flask_cors import CORS
from api.routes import setup_routes
from flask import Blueprint
from prometheus_flask_exporter import PrometheusMetrics


def create_app(config_name='default'):
    app = Flask(__name__)
    CORS(app)
    app.config.from_object(config[config_name])
    metrics = PrometheusMetrics(app)

    init_db(app)

    # Blueprint işlemleri
    api_blueprint = Blueprint('api', __name__)
    api = Api(
        api_blueprint,
        version='1.0',
        title='Hotel Service API',
        description='Hotel service swagger API',
        doc='/'
    )
    setup_routes(api, api_blueprint)
    app.register_blueprint(api_blueprint)

    return app


# 🔥 BURASI EN SONDA OLMALI
if __name__ == '__main__':
    app = create_app()
    app.run(host='0.0.0.0', port=5050)
