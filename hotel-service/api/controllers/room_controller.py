from flask_restx import Resource, Namespace, reqparse, fields
from flask import request
from services.room_service import RoomService
from models.room import get_room_model, get_room_list_model

# Namespace
room_ns = Namespace('rooms', description='Room operations')

# Service
room_service = RoomService()

# Swagger models
room_model = get_room_model(room_ns)
room_list_model = get_room_list_model(room_ns)

# Room status update model
status_update_model = room_ns.model('StatusUpdate', {
    'status': fields.String(required=True, description='New room status')
})

# Utility: extract filters
def extract_filters(args, allowed_keys):
    return {k: v for k, v in args.items() if k in allowed_keys and v is not None}

# Utility: serialize room
def serialize_room(room):
    room['id'] = str(room['_id'])
    del room['_id']
    return room

# Common room filters
room_parser = reqparse.RequestParser()
room_parser.add_argument('hotel_id', type=str)
room_parser.add_argument('room_type', type=str)
room_parser.add_argument('min_price', type=float)
room_parser.add_argument('max_price', type=float)
room_parser.add_argument('capacity', type=int)
room_parser.add_argument('status', type=str)
room_parser.add_argument('page', type=int, default=1)
room_parser.add_argument('per_page', type=int, default=20)

# ------------------------------------------
@room_ns.route('')
class RoomList(Resource):
    @room_ns.doc('list_rooms', description='List all rooms with optional filters and pagination')
    @room_ns.expect(room_parser)
    @room_ns.marshal_with(room_list_model)
    def get(self):
        args = room_parser.parse_args()
        filters = extract_filters(args, ['hotel_id', 'room_type', 'min_price', 'max_price', 'capacity', 'status'])
        page, per_page = args['page'], args['per_page']
        rooms, total = room_service.get_rooms(filters, page, per_page)

        # Convert _id to id
        rooms = [serialize_room(room) for room in rooms]

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
        result = room_service.create_room(data)
        if result.get('error'):
            return {"error": result['error']}, result.get('status_code', 500)
        return {"message": "Room created successfully", "id": result['id']}, 201

# ------------------------------------------
@room_ns.route('/<string:room_id>')
@room_ns.param('room_id', 'Room ID')
@room_ns.response(404, 'Room not found')
class Room(Resource):
    @room_ns.doc('get_room')
    @room_ns.marshal_with(room_model)
    def get(self, room_id):
        """Get a specific room by ID"""
        room = room_service.get_room_by_id(room_id)
        if room:
            return serialize_room(room)
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
