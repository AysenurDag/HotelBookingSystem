from flask import Blueprint
from api.controllers.hotel_controller import hotel_routes
from api.controllers.room_controller import room_routes

api_blueprint = Blueprint('api', __name__)

# Register route blueprints
api_blueprint.register_blueprint(hotel_routes, url_prefix='/hotels')
api_blueprint.register_blueprint(room_routes, url_prefix='/rooms')

# Health check endpoint
@api_blueprint.route('/health', methods=['GET'])
def health_check():
    return {"status": "ok", "service": "hotel-service"}

@api_blueprint.route('/')
def hello():
    return "Hello, World!"