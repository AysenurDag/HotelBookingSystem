from flask_restx import Resource, Namespace, reqparse
from flask import request
from services.room_service import RoomService
from models.room import get_room_model, get_room_list_model

# Create namespace
room_ns = Namespace('rooms', description='Room operations')

# Create the service instance
room_service = RoomService()

# Define the models for Swagger documentation
room_model = get_room_model(room_ns)
room_list_model = get_room_list_model(room_ns)

# Define status update model
status_update_model = room_ns.model('StatusUpdate', {
    'status': room_ns.fields.String(required=True, description='New room status')
})

# Create parsers for query parameters
room_parser = reqparse.RequestParser()
room_parser.add_argument('hotel_id', type=str, help='Filter by hotel ID')
room_parser.add_argument('room_type', type=str, help='Filter by room type')
room_parser.add_argument('min_price', type=float, help='Filter by minimum price')
room_parser.add_argument('max_price', type=float, help='Filter by maximum price')
room_parser.add_argument('capacity', type=int, help='Filter by room capacity')
room_parser.add_argument('status', type=str, help='Filter by room status')
room_parser.add_argument('page', type=int, default=1, help='Page number')
room_parser.add_argument('per_page', type=int, default=20, help='Items per page')

@room_ns.route('')
class RoomList(Resource):
    @room_ns.doc('list_rooms')
    @room_ns.expect(room_parser)
    @room_ns.marshal_with(room_list_model)
    def get(self):
        """List all rooms with optional filtering"""
        args = room_parser.parse_args()
        
        # Query parameters for filtering
        filters = {}
        if args['hotel_id']:
            filters['hotel_id'] = args['hotel_id']
        if args['room_type']:
            filters['room_type'] = args['room_type']
        if args['min_price']:
            filters['min_price'] = args['min_price']
        if args['max_price']:
            filters['max_price'] = args['max_price']
        if args['capacity']:
            filters['capacity'] = args['capacity']
        if args['status']:
            filters['status'] = args['status']
        
        # Pagination
        page = args['page']
        per_page = args['per_page']
        
        rooms, total = room_service.get_rooms(filters, page, per_page)
        
        return {
            "data": rooms,
            "meta": {
                "page": page,
                "per_page": per_page,
                "total": total
            }
        }
    
    @room_ns.doc('create_room')
    @room_ns.expect(room_model)
    @room_ns.response(201, 'Room created successfully')
    @room_ns.response(400, 'Validation error')
    @room_ns.response(500, 'Internal server error')
    def post(self):
        """Create a new room"""
        data = request.json
        
        # Add validation logic here if needed
        # validation_result = validate_room(data)
        # if validation_result:
        #     return {"error": validation_result}, 400
            
        result = room_service.create_room(data)
        if result.get('error'):
            return {"error": result['error']}, result.get('status_code', 500)
        
        return {"message": "Room created successfully", "id": result['id']}, 201

@room_ns.route('/<string:room_id>')
@room_ns.param('room_id', 'The room identifier')
@room_ns.response(404, 'Room not found')
class Room(Resource):
    @room_ns.doc('get_room')
    @room_ns.marshal_with(room_model)
    def get(self, room_id):
        """Get a specific room by ID"""
        room = room_service.get_room_by_id(room_id)
        if room:
            return room
        return {"error": "Room not found"}, 404
    
    @room_ns.doc('update_room')
    @room_ns.expect(room_model)
    @room_ns.response(200, 'Room updated successfully')
    @room_ns.response(500, 'Internal server error')
    def put(self, room_id):
        """Update a room"""
        data = request.json
        
        result = room_service.update_room(room_id, data)
        if result.get('error'):
            return {"error": result['error']}, result.get('status_code', 500)
        
        return {"message": "Room updated successfully"}
    
    @room_ns.doc('delete_room')
    @room_ns.response(200, 'Room deleted successfully')
    @room_ns.response(500, 'Internal server error')
    def delete(self, room_id):
        """Delete a room"""
        result = room_service.delete_room(room_id)
        if result.get('error'):
            return {"error": result['error']}, result.get('status_code', 500)
        
        return {"message": "Room deleted successfully"}

@room_ns.route('/<string:room_id>/status')
@room_ns.param('room_id', 'The room identifier')
class RoomStatus(Resource):
    @room_ns.doc('update_room_status')
    @room_ns.expect(status_update_model)
    @room_ns.response(200, 'Room status updated successfully')
    @room_ns.response(400, 'Missing status field')
    @room_ns.response(500, 'Internal server error')
    def put(self, room_id):
        """Update room status"""
        data = request.json
        
        if not data or 'status' not in data:
            return {"error": "Status field is required"}, 400
            
        result = room_service.update_room_status(room_id, data['status'])
        if result.get('error'):
            return {"error": result['error']}, result.get('status_code', 500)
        
        return {"message": f"Room status updated to {data['status']}"}