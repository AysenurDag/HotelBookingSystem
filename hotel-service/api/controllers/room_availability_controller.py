from flask_restx import Resource, Namespace, fields
from flask import request
from services.room_availability_service import RoomAvailabilityService

# Namespace tanımı
room_availability_ns = Namespace('room-availability', description='Room availability operations')
availability_service = RoomAvailabilityService()

# Swagger Model
availability_model = room_availability_ns.model('RoomAvailability', {
    'roomId': fields.String(required=True, description='Room ID'),
    'dates': fields.List(fields.String, required=True, description='List of ISO format dates (e.g. "2025-06-01")'),
    'isAvailable': fields.Integer(required=True, description='-1: unavailable, 0: pending, 1: available', enum=[-1, 0, 1])
})

availability_response_model = room_availability_ns.model('RoomAvailabilityResponse', {
    'id': fields.String
})

# ------------------------------------------
# Create availability entry
@room_availability_ns.route('')
class RoomAvailabilityList(Resource):
    @room_availability_ns.doc('create_availability')
    @room_availability_ns.expect(availability_model)
    @room_availability_ns.response(201, 'Availability created successfully', availability_response_model)
    @room_availability_ns.response(400, 'Validation error')
    @room_availability_ns.response(500, 'Internal server error')
    def post(self):
        data = request.json
        result = availability_service.create_availability(data)
        if result.get('error'):
            return {"error": result["error"]}, result.get("status_code", 500)
        return {"id": result["id"]}, 201

# ------------------------------------------
# List by room_id
@room_availability_ns.route('/room/<string:room_id>')
@room_availability_ns.param('room_id', 'Room ID')
class RoomAvailabilityByRoom(Resource):
    @room_availability_ns.doc('get_availability_by_room')
    @room_availability_ns.marshal_list_with(availability_model)
    def get(self, room_id):
        result = availability_service.get_availability_by_room(room_id)
        if isinstance(result, dict) and result.get("error"):
            return {"error": result["error"]}, result.get("status_code", 500)
        return result

# ------------------------------------------
# Update & Delete
@room_availability_ns.route('/<string:availability_id>')
@room_availability_ns.param('availability_id', 'Availability ID')
class RoomAvailability(Resource):
    @room_availability_ns.doc('update_availability')
    @room_availability_ns.expect(availability_model)
    @room_availability_ns.response(200, 'Availability updated successfully')
    @room_availability_ns.response(404, 'Availability not found')
    @room_availability_ns.response(500, 'Internal server error')
    def put(self, availability_id):
        data = request.json
        result = availability_service.update_availability(availability_id, data)
        if result.get('error'):
            return {"error": result['error']}, result.get('status_code', 500)
        return {"message": "Availability updated successfully"}

    @room_availability_ns.doc('delete_availability')
    @room_availability_ns.response(200, 'Availability deleted successfully')
    @room_availability_ns.response(404, 'Availability not found')
    @room_availability_ns.response(500, 'Internal server error')
    def delete(self, availability_id):
        result = availability_service.delete_availability(availability_id)
        if result.get('error'):
            return {"error": result['error']}, result.get('status_code', 500)
        return {"message": "Availability deleted successfully"}
