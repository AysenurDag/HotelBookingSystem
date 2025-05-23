from flask import Flask
from database import mongo, init_db
from bson.objectid import ObjectId
from datetime import datetime
from config import config
import os

def migrate_rooms():
    app = Flask(__name__)
    app.config.from_object(config['development'])
    init_db(app)
    
    # Get all hotels
    hotels = list(mongo.db.hotels.find())
    
    # Array to store all rooms
    all_rooms = []
    
    # Extract rooms from each hotel
    for hotel in hotels:
        hotel_id = hotel['_id']
        
        if 'rooms' in hotel:
            for room in hotel['rooms']:
                # Create new room document
                room_doc = {
                    'hotel_id': str(hotel_id),  # Convert ObjectId to string
                    'room_number': room['number'],
                    'room_type': room.get('type', 'standard'),
                    'capacity': room.get('capacity', 2),
                    'price_per_night': room.get('price_per_night', room.get('price', 0)),
                    'beds': room.get('beds', []),
                    'amenities': room.get('amenities', []),
                    'accessibility_features': [],
                    'status': room.get('status', 'available'),
                    'images': [],
                    'created_at': datetime.utcnow(),
                    'updated_at': datetime.utcnow()
                }
                
                all_rooms.append(room_doc)
    
    # Insert all rooms into the rooms collection
    if all_rooms:
        result = mongo.db.rooms.insert_many(all_rooms)
        print(f"Successfully migrated {len(result.inserted_ids)} rooms")
    else:
        print("No rooms to migrate")
    
    # Optionally, remove the rooms arrays from the hotels collection
    # Uncomment if you want to remove rooms from hotels after migration
    # mongo.db.hotels.update_many({}, {'$unset': {'rooms': ''}})

if __name__ == '__main__':
    migrate_rooms()