from flask_restx import Resource, Namespace, reqparse, fields
from flask import request
from services.room_service import RoomService

# Namespace
room_ns = Namespace('rooms', description='Room operations')
room_service = RoomService()

# Swagger Models
room_model = room_ns.model('Room', {
    'id': fields.String(readOnly=True),
    'hotel_id': fields.String(required=True),
    'room_number': fields.String(required=True),
    'type': fields.String(required=True, enum=['single', 'double', 'twin', 'suite', 'deluxe']),
    'capacity': fields.Integer(required=True),
    'price_per_night': fields.Float(required=True),
    'description': fields.String,
    'amenities': fields.List(fields.String),
    'images': fields.List(fields.String),
    'is_active': fields.Boolean
})

room_list_model = room_ns.model('RoomListResponse', {
    'data': fields.List(fields.Nested(room_model)),
    'meta': fields.Raw
})

# Request Parser
room_parser = reqparse.RequestParser()
room_parser.add_argument('hotel_id', type=str)
room_parser.add_argument('type', type=str)
room_parser.add_argument('min_price', type=float)
room_parser.add_argument('max_price', type=float)
room_parser.add_argument('capacity', type=int)
room_parser.add_argument('status', type=str, choices=('active', 'inactive'))
room_parser.add_argument('page', type=int, default=1)
room_parser.add_argument('per_page', type=int, default=20)

# ------------------------------------------
# List & Create Rooms
@room_ns.route('')
class RoomList(Resource):
    @room_ns.doc('list_rooms', description='List all rooms with optional filters and pagination')
    @room_ns.expect(room_parser)
    @room_ns.marshal_with(room_list_model)
    def get(self):
        args = room_parser.parse_args()
        filters = {k: v for k, v in args.items() if k in ['hotel_id', 'type', 'min_price', 'max_price', 'capacity', 'status'] and v is not None}
        page, per_page = args['page'], args['per_page']
        
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
    @room_ns.response(409, 'Room already exists')
    @room_ns.response(500, 'Internal server error')
    def post(self):
        data = request.json
        result = room_service.create_room(data)
        if result.get('error'):
            return {"error": result['error']}, result.get('status_code', 500)
        return {"message": "Room created successfully", "id": result['id']}, 201

# ------------------------------------------
# Room by ID: Get, Update, Delete
@room_ns.route('/<string:room_id>')
@room_ns.param('room_id', 'Room ID')
class Room(Resource):
    @room_ns.doc('get_room')
    @room_ns.marshal_with(room_model)
    def get(self, room_id):
        room = room_service.get_room_by_id(room_id)
        if isinstance(room, dict) and room.get("error"):
            return {"error": room["error"]}, room.get("status_code", 500)
        if room:
            return room
        return {"error": "Room not found"}, 404

    @room_ns.doc('update_room')
    @room_ns.expect(room_model)
    @room_ns.response(200, 'Room updated successfully')
    @room_ns.response(404, 'Room not found')
    @room_ns.response(500, 'Internal server error')
    def put(self, room_id):
        data = request.json
        result = room_service.update_room(room_id, data)
        if result.get('error'):
            return {"error": result['error']}, result.get('status_code', 500)
        return {"message": "Room updated successfully"}

    @room_ns.doc('delete_room')
    @room_ns.response(200, 'Room deleted (soft delete) successfully')
    @room_ns.response(404, 'Room not found')
    @room_ns.response(500, 'Internal server error')
    def delete(self, room_id):
        result = room_service.delete_room(room_id)
        if result.get('error'):
            return {"error": result['error']}, result.get('status_code', 500)
        return {"message": "Room deleted successfully"}
