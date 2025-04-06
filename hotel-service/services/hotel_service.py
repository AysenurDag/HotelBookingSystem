from flask_pymongo import PyMongo
from flask import current_app, g
from bson.objectid import ObjectId
from datetime import datetime
from services.messaging import publish_message
from database import mongo  

class HotelService:
    def __init__(self):
        pass
        
    @property
    def db(self):
        return mongo.db
    
    def get_hotels(self, filters=None, page=1, per_page=10):
        filters = filters or {}
        query = {}
        
        if 'city' in filters:
            query['city'] = {'$regex': filters['city'], '$options': 'i'}
        if 'rating' in filters:
            query['rating'] = {'$gte': filters['rating']}
            
        # Handle other filters...
        
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
    
    def get_hotel_amenities(self, hotel_id):
        try:
            hotel = self.db.hotels.find_one(
                {'_id': ObjectId(hotel_id)}, 
                {'amenities': 1}
            )
            
            if not hotel:
                return []
                
            return hotel.get('amenities', [])
        except Exception as e:
            current_app.logger.error(f"Error getting amenities: {str(e)}")
            return []
    
    def get_hotel_accessibility(self, hotel_id):
        try:
            hotel = self.db.hotels.find_one(
                {'_id': ObjectId(hotel_id)}, 
                {'accessibility_features': 1}
            )
            
            if not hotel:
                return []
                
            return hotel.get('accessibility_features', [])
        except Exception as e:
            current_app.logger.error(f"Error getting accessibility features: {str(e)}")
            return []