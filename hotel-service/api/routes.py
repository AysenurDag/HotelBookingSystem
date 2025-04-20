from flask import Blueprint, jsonify

# Create the main API blueprint
api_blueprint = Blueprint('api', __name__)

def setup_routes(api):
    # Import controllers
    from api.controllers.hotel_controller import hotel_ns, HotelList, Hotel
    from api.controllers.room_controller import room_ns
    
    # Add health check endpoints directly to the blueprint (no Swagger)
    @api_blueprint.route('/health', methods=['GET'])
    def health_check():
        return jsonify({"status": "ok", "service": "hotel-service"})
    
    @api_blueprint.route('/', methods=['GET'])
    def check():
        return jsonify({"status": "ok"})
    
    # Register namespaces with the API
    api.add_namespace(hotel_ns, path='/hotels')
    api.add_namespace(room_ns, path='/rooms')

    # hotel_ns.add_resource(HotelList, '')
    # hotel_ns.add_resource(Hotel, '/<string:hotel_id>')
    
    return api