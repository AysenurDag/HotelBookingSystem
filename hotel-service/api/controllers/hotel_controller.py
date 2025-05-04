from flask_restx import Resource, Namespace, reqparse
from flask import request
from services.hotel_service import HotelService
from models.hotel import (
    get_hotel_model, 
    get_hotel_list_model
)
from models.room import get_room_model, get_room_list_model

# Create namespace
hotel_ns = Namespace('hotels', description='Hotel operations')

# Create the service instance
hotel_service = HotelService()

# Define the models for Swagger documentation
hotel_model = get_hotel_model(hotel_ns)
hotel_list_model = get_hotel_list_model(hotel_ns)
room_model = get_room_model(hotel_ns)
room_list_model = get_room_list_model(hotel_ns)

# Create parsers for query parameters
hotel_parser = reqparse.RequestParser()
hotel_parser.add_argument('city', type=str, help='Filter by city')
hotel_parser.add_argument('rating', type=int, help='Filter by minimum rating')
hotel_parser.add_argument('page', type=int, default=1, help='Page number')
hotel_parser.add_argument('per_page', type=int, default=10, help='Items per page')

room_parser = reqparse.RequestParser()
room_parser.add_argument('room_type', type=str, help='Filter by room type')
room_parser.add_argument('min_price', type=float, help='Filter by minimum price')
room_parser.add_argument('max_price', type=float, help='Filter by maximum price')
room_parser.add_argument('capacity', type=int, help='Filter by room capacity')
room_parser.add_argument('status', type=str, help='Filter by room status')
room_parser.add_argument('page', type=int, default=1, help='Page number')
room_parser.add_argument('per_page', type=int, default=20, help='Items per page')

location_parser = reqparse.RequestParser()
location_parser.add_argument('city', type=str, help='Filter by city')
location_parser.add_argument('country', type=str, help='Filter by country')
location_parser.add_argument('page', type=int, default=1, help='Page number')
location_parser.add_argument('per_page', type=int, default=10, help='Items per page')

@hotel_ns.route('')
class HotelList(Resource):
    @hotel_ns.doc('list_hotels')
    @hotel_ns.expect(hotel_parser)
    @hotel_ns.marshal_with(hotel_list_model)
    def get(self):
        """List all hotels with optional filtering"""
        args = hotel_parser.parse_args()
        
        # Query parameters for filtering
        filters = {}
        if args['city']:
            filters['city'] = args['city']
        if args['rating']:
            filters['rating'] = args['rating']
        
        # Pagination
        page = args['page']
        per_page = args['per_page']
        
        hotels, total = hotel_service.get_hotels(filters, page, per_page)
        
        return {
            "data": hotels,
            "meta": {
                "page": page,
                "per_page": per_page,
                "total": total
            }
        }
    
    @hotel_ns.doc('create_hotel')
    @hotel_ns.expect(hotel_model)
    @hotel_ns.response(201, 'Hotel created successfully')
    @hotel_ns.response(400, 'Validation error')
    @hotel_ns.response(500, 'Internal server error')
    def post(self):
        """Create a new hotel"""
        data = request.json
        
        # Add validation logic here if needed
            
        result = hotel_service.create_hotel(data)
        if result.get('error'):
            return {"error": result['error']}, result.get('status_code', 500)
        
        return {"message": "Hotel created successfully", "id": result['id']}, 201

@hotel_ns.route('/<string:hotel_id>')
@hotel_ns.param('hotel_id', 'The hotel identifier')
@hotel_ns.response(404, 'Hotel not found')
class Hotel(Resource):
    @hotel_ns.doc('get_hotel')
    @hotel_ns.marshal_with(hotel_model)
    def get(self, hotel_id):
        """Get a specific hotel by ID"""
        hotel = hotel_service.get_hotel_by_id(hotel_id)
        if hotel:
            return hotel
        hotel_ns.abort(404, f"Hotel {hotel_id} not found")
    
    @hotel_ns.doc('update_hotel')
    @hotel_ns.expect(hotel_model)
    @hotel_ns.response(200, 'Hotel updated successfully')
    @hotel_ns.response(500, 'Internal server error')
    def put(self, hotel_id):
        """Update a hotel"""
        data = request.json
        
        result = hotel_service.update_hotel(hotel_id, data)
        if result.get('error'):
            return {"error": result['error']}, result.get('status_code', 500)
        
        return {"message": "Hotel updated successfully"}
    
    @hotel_ns.doc('delete_hotel')
    @hotel_ns.response(200, 'Hotel deleted successfully')
    @hotel_ns.response(500, 'Internal server error')
    def delete(self, hotel_id):
        """Delete a hotel"""
        result = hotel_service.delete_hotel(hotel_id)
        if result.get('error'):
            return {"error": result['error']}, result.get('status_code', 500)
        
        return {"message": "Hotel deleted successfully"}

@hotel_ns.route('/<string:hotel_id>/rooms')
@hotel_ns.param('hotel_id', 'The hotel identifier')
class HotelRooms(Resource):
    @hotel_ns.doc('get_hotel_rooms')
    @hotel_ns.expect(room_parser)
    @hotel_ns.marshal_with(room_list_model)
    def get(self, hotel_id):
        """Get rooms for a specific hotel with optional filtering"""
        args = room_parser.parse_args()
        
        # Query parameters for filtering
        filters = {}
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
        
        rooms, total = hotel_service.get_hotel_rooms(hotel_id, filters, page, per_page)
        
        return {
            "data": rooms,
            "meta": {
                "page": page,
                "per_page": per_page,
                "total": total,
                "hotel_id": hotel_id
            }
        }

@hotel_ns.route('/search/location')
class HotelLocationSearch(Resource):
    @hotel_ns.doc('search_hotels_by_location')
    @hotel_ns.expect(location_parser)
    @hotel_ns.marshal_with(hotel_list_model)
    @hotel_ns.response(400, 'Missing location parameters')
    def get(self):
        """Search hotels by location (city and/or country)"""
        args = location_parser.parse_args()
        city = args['city']
        country = args['country']
        
        if not city and not country:
            return {"error": "At least one location parameter (city or country) is required"}, 400
        
        # Pagination
        page = args['page']
        per_page = args['per_page']
        
        hotels, total = hotel_service.search_hotels_by_location(city, country, page, per_page)
        
        return {
            "data": hotels,
            "meta": {
                "page": page,
                "per_page": per_page,
                "total": total,
                "filters": {
                    "city": city,
                    "country": country
                }
            }
        }