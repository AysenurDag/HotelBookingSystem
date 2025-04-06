from flask import Blueprint, request, jsonify
from services.hotel_service import HotelService
# from utils.validators import validate_hotel

hotel_routes = Blueprint('hotel_routes', __name__)
hotel_service = HotelService()

@hotel_routes.route('', methods=['GET'])
def get_hotels():
    # Query parameters for filtering
    filters = {}
    if 'city' in request.args:
        filters['city'] = request.args.get('city')
    if 'rating' in request.args:
        filters['rating'] = float(request.args.get('rating'))
    
    # Pagination
    page = int(request.args.get('page', 1))
    per_page = int(request.args.get('per_page', 10))
    
    hotels, total = hotel_service.get_hotels(filters, page, per_page)
    
    return jsonify({
        "data": hotels,
        "meta": {
            "page": page,
            "per_page": per_page,
            "total": total
        }
    })

@hotel_routes.route('/<string:hotel_id>', methods=['GET'])
def get_hotel(hotel_id):
    hotel = hotel_service.get_hotel_by_id(hotel_id)
    if hotel:
        return jsonify(hotel)
    return jsonify({"error": "Hotel not found"}), 404

@hotel_routes.route('', methods=['POST'])
def create_hotel():
    data = request.json
    
    # # Validate input
    # validation_result = validate_hotel(data)
    # if validation_result:
    #     return jsonify({"error": validation_result}), 400
        
    result = hotel_service.create_hotel(data)
    if result.get('error'):
        return jsonify({"error": result['error']}), result.get('status_code', 500)
    
    return jsonify({"message": "Hotel created successfully", "id": result['id']}), 201

@hotel_routes.route('/<string:hotel_id>', methods=['PUT'])
def update_hotel(hotel_id):
    data = request.json
    
    result = hotel_service.update_hotel(hotel_id, data)
    if result.get('error'):
        return jsonify({"error": result['error']}), result.get('status_code', 500)
    
    return jsonify({"message": "Hotel updated successfully"})

@hotel_routes.route('/<string:hotel_id>', methods=['DELETE'])
def delete_hotel(hotel_id):
    result = hotel_service.delete_hotel(hotel_id)
    if result.get('error'):
        return jsonify({"error": result['error']}), result.get('status_code', 500)
    
    return jsonify({"message": "Hotel deleted successfully"})

# Route for hotel amenities
@hotel_routes.route('/<string:hotel_id>/amenities', methods=['GET'])
def get_hotel_amenities(hotel_id):
    amenities = hotel_service.get_hotel_amenities(hotel_id)
    return jsonify(amenities)

# Route for hotel accessibility features
@hotel_routes.route('/<string:hotel_id>/accessibility', methods=['GET'])
def get_hotel_accessibility(hotel_id):
    accessibility = hotel_service.get_hotel_accessibility(hotel_id)
    return jsonify(accessibility)