from flask import Blueprint, request, jsonify
from services.room_service import RoomService
# from utils.validators import validate_room

room_routes = Blueprint('room_routes', __name__)
room_service = RoomService()

@room_routes.route('', methods=['GET'])
def get_rooms():
    # Query parameters for filtering
    filters = {}
    if 'hotel_id' in request.args:
        filters['hotel_id'] = request.args.get('hotel_id')
    if 'room_type' in request.args:
        filters['room_type'] = request.args.get('room_type')
    if 'min_price' in request.args:
        filters['min_price'] = float(request.args.get('min_price'))
    if 'max_price' in request.args:
        filters['max_price'] = float(request.args.get('max_price'))
    if 'capacity' in request.args:
        filters['capacity'] = int(request.args.get('capacity'))
    if 'status' in request.args:
        filters['status'] = request.args.get('status')
    
    # Pagination
    page = int(request.args.get('page', 1))
    per_page = int(request.args.get('per_page', 20))
    
    rooms, total = room_service.get_rooms(filters, page, per_page)
    
    return jsonify({
        "data": rooms,
        "meta": {
            "page": page,
            "per_page": per_page,
            "total": total
        }
    })

@room_routes.route('/<string:room_id>', methods=['GET'])
def get_room(room_id):
    room = room_service.get_room_by_id(room_id)
    if room:
        return jsonify(room)
    return jsonify({"error": "Room not found"}), 404

@room_routes.route('', methods=['POST'])
def create_room():
    data = request.json
    
    # # Validate input
    # validation_result = validate_room(data)
    # if validation_result:
    #     return jsonify({"error": validation_result}), 400
        
    result = room_service.create_room(data)
    if result.get('error'):
        return jsonify({"error": result['error']}), result.get('status_code', 500)
    
    return jsonify({"message": "Room created successfully", "id": result['id']}), 201

@room_routes.route('/<string:room_id>', methods=['PUT'])
def update_room(room_id):
    data = request.json
    
    result = room_service.update_room(room_id, data)
    if result.get('error'):
        return jsonify({"error": result['error']}), result.get('status_code', 500)
    
    return jsonify({"message": "Room updated successfully"})

@room_routes.route('/<string:room_id>', methods=['DELETE'])
def delete_room(room_id):
    result = room_service.delete_room(room_id)
    if result.get('error'):
        return jsonify({"error": result['error']}), result.get('status_code', 500)
    
    return jsonify({"message": "Room deleted successfully"})

@room_routes.route('/<string:room_id>/status', methods=['PUT'])
def update_room_status(room_id):
    data = request.json
    if 'status' not in data:
        return jsonify({"error": "Status field is required"}), 400
        
    result = room_service.update_room_status(room_id, data['status'])
    if result.get('error'):
        return jsonify({"error": result['error']}), result.get('status_code', 500)
    
    return jsonify({"message": f"Room status updated to {data['status']}"})