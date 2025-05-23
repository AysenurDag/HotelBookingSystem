from flask import Flask
from config import config
from database import init_db
from flask_restx import Api
from flask_cors import CORS
from api.routes import setup_routes  # sadece setup_routes al
from flask import Blueprint

def create_app(config_name='default'):
    app = Flask(__name__)
    CORS(app)
    app.config.from_object(config[config_name])

    init_db(app)

    # 1. Blueprint'i burada oluştur
    api_blueprint = Blueprint('api', __name__, url_prefix='/api')


    # 2. API nesnesi Blueprint'e bağlanır
    api = Api(
        api_blueprint,
        version='1.0',
        title='Hotel Service API',
        description='Hotel service swagger API',
        doc='/'
    )

    # 3. Rotaları blueprint'e ekle
    setup_routes(api, api_blueprint)

    # 4. Blueprint'i en son Flask'a ekle
    app.register_blueprint(api_blueprint)

    return app