from flask_restx import Resource, Namespace, fields
from flask import request
from flask import current_app
from services.room_availability_service import RoomAvailabilityService

# Namespace tanımı
room_availability_ns = Namespace('room-availability', description='Room availability operations')
availability_service = RoomAvailabilityService()

# Swagger Modelleri
availability_model = room_availability_ns.model('RoomAvailability', {
    'roomId': fields.String(required=True, description='Room ID'),
    'isAvailable': fields.Integer(required=True, description='-1: unavailable, 0: pending, 1: available', enum=[-1, 0, 1]),
    'start_date': fields.String(required=False, description='Start date in ISO format (e.g. 2025-06-01)'),
    'end_date': fields.String(required=False, description='End date in ISO format (e.g. 2025-06-05)'),
    'dates': fields.List(fields.String, required=False, description='List of specific dates (e.g. ["2025-06-01", "2025-06-03"])')
})

availability_response_model = room_availability_ns.model('RoomAvailabilityResponse', {
    'inserted_ids': fields.List(fields.String, description='List of inserted availability document IDs')
})

availability_output_model = room_availability_ns.model('AvailabilityOutput', {
    'id': fields.String(description='Document ID'),
    'room_id': fields.String(description='Room ID'),
    'date': fields.String(description='Availability date in ISO format'),
    'isAvailable': fields.Integer(description='-1: unavailable, 0: pending, 1: available')
})

availability_update_model = room_availability_ns.model('AvailabilityUpdate', {
    'isAvailable': fields.Integer(required=False, description='-1: unavailable, 0: pending, 1: available'),
    'date': fields.String(required=False, description='Date in ISO format')
})

# ------------------------------------------
# Create availability entry
@room_availability_ns.route('')
class RoomAvailabilityList(Resource):
    @room_availability_ns.doc('create_availability', description="Create room availability using start_date, date range, or list of dates")
    @room_availability_ns.expect(availability_model)
    @room_availability_ns.response(201, 'Availability created successfully', availability_response_model)
    @room_availability_ns.response(400, 'Validation error')
    @room_availability_ns.response(500, 'Internal server error')
    def post(self):
    
        data = request.json
        current_app.logger.info(f"📥 Gelen veri (POST): {data}")
        result = availability_service.create_availability(data)
        if result.get('error'):
            return {"error": result["error"]}, result.get("status_code", 500)
        return {"inserted_ids": result["inserted_ids"]}, 201

# ------------------------------------------
# List by room_id
@room_availability_ns.route('/room/<string:room_id>')
class RoomAvailabilityByRoom(Resource):
    @room_availability_ns.doc('get_availability_by_room')
    @room_availability_ns.param('start_date', 'Start date in ISO format')
    @room_availability_ns.param('end_date', 'End date in ISO format')
    @room_availability_ns.marshal_list_with(availability_output_model)
    def get(self, room_id):
        start_date = request.args.get("start_date")
        end_date = request.args.get("end_date")
        result = availability_service.get_availability_by_room(room_id, start_date, end_date)

        if isinstance(result, dict) and result.get("error"):
            return {"error": result["error"]}, result.get("status_code", 500)
        return result

# ------------------------------------------
# Update & Delete
@room_availability_ns.route('/<string:availability_id>')
@room_availability_ns.param('availability_id', 'Availability ID')
class RoomAvailability(Resource):
    @room_availability_ns.doc('update_availability')
    @room_availability_ns.expect(availability_update_model)
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
