from flask_pymongo import PyMongo
from flask import current_app, g
from bson.objectid import ObjectId
from datetime import datetime

from database import mongo
from rabbitmq_utils import publish_message  

class HotelService:
    def __init__(self):
        pass
        
    @property
    def db(self):
        return mongo.db
    
    def get_hotels(self, filters=None, page=1, per_page=10):
        filters = filters or {}
        query = {}
        
        # Update filter handling to match schema
        if 'city' in filters:
            query['address.city'] = {'$regex': filters['city'], '$options': 'i'}
        if 'country' in filters:
            query['address.country'] = {'$regex': filters['country'], '$options': 'i'}
        if 'rating' in filters:
            query['rating'] = {'$gte': filters['rating']}
        if 'amenities' in filters:
            if isinstance(filters['amenities'], list):
                query['amenities'] = {'$all': filters['amenities']}
            else:
                query['amenities'] = filters['amenities']
            
        skip = (page - 1) * per_page
        
        # Get total count for pagination
        total = self.db.hotels.count_documents(query)
        
        # Get paginated results
        hotels = list(self.db.hotels.find(query).skip(skip).limit(per_page))
        
        # Convert ObjectId to string for JSON serialization
        for hotel in hotels:
            hotel['_id'] = str(hotel['_id'])
            
        return hotels, total
    
    def get_hotel_by_id(self, hotel_id):
        try:
            hotel = self.db.hotels.find_one({'_id': ObjectId(hotel_id)})
            if hotel:
                hotel['_id'] = str(hotel['_id'])
                return hotel
            return None
        except Exception as e:
            current_app.logger.error(f"Error getting hotel: {str(e)}")
            return None
    
    def create_hotel(self, hotel_data):
        try:
            # Validate required fields as per schema
            required_fields = ['name', 'address', 'rating']
            for field in required_fields:
                if field not in hotel_data:
                    return {'error': f'Missing required field: {field}', 'status_code': 400}
            
            # Validate address structure
            address_fields = ['street', 'city', 'country']
            if not all(field in hotel_data['address'] for field in address_fields):
                return {'error': f'Address must include {", ".join(address_fields)}', 'status_code': 400}
            
            # Validate rating range
            if not (1 <= hotel_data['rating'] <= 5):
                return {'error': 'Rating must be between 1 and 5', 'status_code': 400}
            
            # Validate contact if provided
            if 'contact' in hotel_data:
                contact_fields = ['phone', 'email']
                if not all(field in hotel_data['contact'] for field in contact_fields):
                    return {'error': f'Contact must include {", ".join(contact_fields)}', 'status_code': 400}
            
            # Check if hotel with same name exists
            existing = self.db.hotels.find_one({'name': hotel_data['name']})
            if existing:
                return {'error': 'Hotel with this name already exists', 'status_code': 409}
            
            # Add timestamps
            hotel_data['created_at'] = datetime.utcnow()
            hotel_data['updated_at'] = hotel_data['created_at']
            
            result = self.db.hotels.insert_one(hotel_data)
            
            # Publish message to RabbitMQ
            hotel_data['_id'] = str(result.inserted_id)
            publish_message('hotel.created', hotel_data)
            
            return {'id': str(result.inserted_id)}
        except Exception as e:
            current_app.logger.error(f"Error creating hotel: {str(e)}")
            return {'error': str(e)}
    
    def update_hotel(self, hotel_id, hotel_data):
        try:
            # Validate rating if provided
            if 'rating' in hotel_data and not (1 <= hotel_data['rating'] <= 5):
                return {'error': 'Rating must be between 1 and 5', 'status_code': 400}
            
            # Validate address structure if provided
            if 'address' in hotel_data:
                address_fields = ['street', 'city', 'country']
                if not all(field in hotel_data['address'] for field in address_fields):
                    return {'error': f'Address must include {", ".join(address_fields)}', 'status_code': 400}
            
            # Validate contact if provided
            if 'contact' in hotel_data:
                contact_fields = ['phone', 'email']
                if not all(field in hotel_data['contact'] for field in contact_fields):
                    return {'error': f'Contact must include {", ".join(contact_fields)}', 'status_code': 400}
            
            # Update timestamp
            hotel_data['updated_at'] = datetime.utcnow()
            
            result = self.db.hotels.update_one(
                {'_id': ObjectId(hotel_id)},
                {'$set': hotel_data}
            )
            
            if result.matched_count == 0:
                return {'error': 'Hotel not found', 'status_code': 404}
                
            if result.modified_count > 0:
                # Publish message to RabbitMQ
                hotel_data['_id'] = hotel_id
                publish_message('hotel.updated', hotel_data)
                
            return {'success': True}
        except Exception as e:
            current_app.logger.error(f"Error updating hotel: {str(e)}")
            return {'error': str(e)}
    
    def delete_hotel(self, hotel_id):
        try:
            # Check if hotel has rooms
            room_count = self.db.rooms.count_documents({'hotel_id': hotel_id})
            if room_count > 0:
                return {'error': 'Cannot delete hotel with existing rooms', 'status_code': 409}
            
            result = self.db.hotels.delete_one({'_id': ObjectId(hotel_id)})
            
            if result.deleted_count == 0:
                return {'error': 'Hotel not found', 'status_code': 404}
                
            # Publish message to RabbitMQ
            publish_message('hotel.deleted', {'_id': hotel_id})
                
            return {'success': True}
        except Exception as e:
            current_app.logger.error(f"Error deleting hotel: {str(e)}")
            return {'error': str(e)}
           
    def search_hotels_by_location(self, city=None, country=None, page=1, per_page=10):
        query = {}
        
        if city:
            query['address.city'] = {'$regex': city, '$options': 'i'}  # Case-insensitive search
        if country:
            query['address.country'] = {'$regex': country, '$options': 'i'}
            
        skip = (page - 1) * per_page
        
        # Get total count for pagination
        total = self.db.hotels.count_documents(query)
        
        # Get paginated results
        hotels = list(self.db.hotels.find(query).skip(skip).limit(per_page))
        
        # Convert ObjectId to string for JSON serialization
        for hotel in hotels:
            hotel['_id'] = str(hotel['_id'])
            
        return hotels, total
        
    def get_hotel_rooms(self, hotel_id, filters=None, page=1, per_page=20):
        """
        Get all rooms associated with a specific hotel with optional filtering.
        
        Args:
            hotel_id (str): The ID of the hotel.
            filters (dict, optional): Optional filters for room query.
            page (int, optional): Page number for pagination. Defaults to 1.
            per_page (int, optional): Number of results per page. Defaults to 20.
            
        Returns:
            tuple: (rooms, total) where rooms is a list of room objects and total is the total count.
        """
        try:
            # Verify hotel exists
            hotel = self.db.hotels.find_one({'_id': ObjectId(hotel_id)})
            if not hotel:
                return [], 0
                
            # Build query
            query = {'hotelId': hotel_id}
            
            # Apply additional filters if provided
            if filters:
                if 'room_type' in filters:
                    query['room_type'] = filters['room_type']
                if 'min_price' in filters and 'max_price' in filters:
                    query['price_per_night'] = {
                        '$gte': filters['min_price'],
                        '$lte': filters['max_price']
                    }
                elif 'min_price' in filters:
                    query['price_per_night'] = {'$gte': filters['min_price']}
                elif 'max_price' in filters:
                    query['price_per_night'] = {'$lte': filters['max_price']}
                if 'capacity' in filters:
                    query['capacity'] = {'$gte': filters['capacity']}
                if 'status' in filters:
                    query['status'] = filters['status']
                    
            skip = (page - 1) * per_page
            
            # Get total count for pagination
            total = self.db.rooms.count_documents(query)
            
            # Get paginated results
            rooms = list(self.db.rooms.find(query).skip(skip).limit(per_page))
            
            # Convert ObjectId to string for JSON serialization
            for room in rooms:
                room['_id'] = str(room['_id'])
                if 'hotel_id' in room and ObjectId.is_valid(room['hotel_id']):
                    room['hotel_id'] = str(room['hotel_id'])
                
            return rooms, total
        except Exception as e:
            current_app.logger.error(f"Error getting hotel rooms: {str(e)}")
            return [], 0