from flask_pymongo import PyMongo
from flask import current_app, g
from bson.objectid import ObjectId
from datetime import datetime
from database import mongo
from rabbitmq_utils import publish_message


class RoomService:
    def __init__(self):
        pass
        
    @property
    def db(self):
        return mongo.db
    
    def get_rooms(self, filters=None, page=1, per_page=20):
        filters = filters or {}
        query = {}
        
        if 'hotel_id' in filters:
            query['hotel_id'] = filters['hotel_id']
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
            
        skip = (page - 1) * per_page
        
        # Get total count for pagination
        total = self.db.rooms.count_documents(query)
        
        # Get paginated results
        rooms = list(self.db.rooms.find(query).skip(skip).limit(per_page))
        
        # Convert ObjectId to string for JSON serialization
        for room in rooms:
            room['_id'] = str(room['_id'])
            
        return rooms, total
    
    def get_room_by_id(self, room_id):
        try:
            room = self.db.rooms.find_one({'_id': ObjectId(room_id)})
            if room:
                room['_id'] = str(room['_id'])
                return room
            return None
        except Exception as e:
            current_app.logger.error(f"Error getting room: {str(e)}")
            return None
    
    def create_room(self, room_data):
        try:
            # Check if hotel exists
            hotel = self.db.hotels.find_one({'_id': ObjectId(room_data['hotel_id'])})
            if not hotel:
                return {'error': 'Hotel not found', 'status_code': 404}
                
            # # Check if room with same number exists in the hotel
            # existing = self.db.rooms.find_one({
            #     'hotel_id': room_data['hotel_id'],
            #     'room_number': room_data['room_number']
            # })
            # if existing:
            #     return {'error': 'Room with this number already exists in this hotel', 'status_code': 409}
            
            # Add timestamps
            
            result = self.db.rooms.insert_one(room_data)
            
            room_data['_id'] = str(result.inserted_id)
            
            return {'id': str(result.inserted_id)}
        except Exception as e:
            current_app.logger.error(f"Error creating room: {str(e)}")
            return {'error': str(e)}
    
    def update_room(self, room_id, room_data):
        try:
            # Update timestamp
            room_data['updated_at'] = datetime.utcnow()
            
            result = self.db.rooms.update_one(
                {'_id': ObjectId(room_id)},
                {'$set': room_data}
            )
            
            if result.matched_count == 0:
                return {'error': 'Room not found', 'status_code': 404}
                
            if result.modified_count > 0:
                room_data['_id'] = room_id
                
            return {'success': True}
        except Exception as e:
            current_app.logger.error(f"Error updating room: {str(e)}")
            return {'error': str(e)}
    
    def delete_room(self, room_id):
        try:
            # Check if room is in use
            room = self.db.rooms.find_one({'_id': ObjectId(room_id)})
           
            result = self.db.rooms.delete_one({'_id': ObjectId(room_id)})
            
            if result.deleted_count == 0:
                return {'error': 'Room not found', 'status_code': 404}
                
                
            return {'success': True}
        except Exception as e:
            current_app.logger.error(f"Error deleting room: {str(e)}")
            return {'error': str(e)}
    