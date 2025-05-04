from flask_restx import Resource, Namespace, reqparse
from flask import request
from services.hotel_service import HotelService
from models.hotel import get_hotel_model, get_hotel_list_model
from models.room import get_room_model, get_room_list_model

# Namespace
hotel_ns = Namespace('hotels', description='Hotel operations')

# Service
hotel_service = HotelService()

# Swagger models
hotel_model = get_hotel_model(hotel_ns)
hotel_list_model = get_hotel_list_model(hotel_ns)
room_model = get_room_model(hotel_ns)
room_list_model = get_room_list_model(hotel_ns)

# --- Common Parsers ---
pagination_parser = reqparse.RequestParser()
pagination_parser.add_argument('page', type=int, default=1, help='Page number')
pagination_parser.add_argument('per_page', type=int, default=10, help='Items per page')

hotel_parser = pagination_parser.copy()
hotel_parser.add_argument('city', type=str, help='Filter by city')
hotel_parser.add_argument('country', type=str, help='Filter by country')
hotel_parser.add_argument('rating', type=int, help='Filter by minimum rating')

# --- Utility function ---
def extract_filters(args, allowed_keys):
    return {k: v for k, v in args.items() if k in allowed_keys and v is not None}

# --- Controllers ---

@hotel_ns.route('/getHotels')
class HotelList(Resource):
    @hotel_ns.doc('list_hotels', description='List all hotels with optional city/rating filters and pagination')
    @hotel_ns.expect(hotel_parser)
    @hotel_ns.marshal_with(hotel_list_model)
    def get(self):
        args = hotel_parser.parse_args()
        filters = extract_filters(args, ['city', 'rating'])
        page, per_page = args['page'], args['per_page']
        hotels, total = hotel_service.get_hotels(filters, page, per_page)
        for hotel in hotels:
            hotel['id'] = str(hotel['_id'])   # 
            del hotel['_id']
        return {"data": hotels, "meta": {"page": page, "per_page": per_page, "total": total}}

@hotel_ns.route('/createHotel')
class CreateHotel(Resource):
    @hotel_ns.doc('create_hotel')
    @hotel_ns.expect(hotel_model)
    @hotel_ns.response(201, 'Hotel created successfully')
    @hotel_ns.response(400, 'Validation error')
    @hotel_ns.response(500, 'Internal server error')
    def post(self):
        data = request.json
        result = hotel_service.create_hotel(data)
        if result.get('error'):
            return {"error": result['error']}, result.get('status_code', 500)
        return {"message": "Hotel created successfully", "id": result['id']}, 201    

@hotel_ns.route('/<string:hotel_id>')
@hotel_ns.param('hotel_id', 'Hotel ID')
class Hotel(Resource):
    @hotel_ns.doc('get_hotel')
    @hotel_ns.marshal_with(hotel_model)
    def get(self, hotel_id):
        hotel = hotel_service.get_hotel_by_id(hotel_id)
        if hotel:
            hotel['id'] = str(hotel['_id'])  
            del hotel['_id']
            return hotel
        hotel_ns.abort(404, f"Hotel {hotel_id} not found")

    @hotel_ns.doc('update_hotel')
    @hotel_ns.expect(hotel_model)
    def put(self, hotel_id):
        data = request.json
        result = hotel_service.update_hotel(hotel_id, data)
        if result.get('error'):
            return {"error": result['error']}, result.get('status_code', 500)
        return {"message": "Hotel updated successfully"}

    @hotel_ns.doc('delete_hotel')
    def delete(self, hotel_id):
        result = hotel_service.delete_hotel(hotel_id)
        if result.get('error'):
            return {"error": result['error']}, result.get('status_code', 500)
        return {"message": "Hotel deleted successfully"}