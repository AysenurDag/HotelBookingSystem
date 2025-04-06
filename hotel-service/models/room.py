from datetime import datetime

class Room:
    def __init__(self, hotel_id, room_number, room_type, capacity, price_per_night,
                 beds=None, amenities=None, accessibility_features=None, 
                 status="available", images=None, size_sqm=None):
        self.hotel_id = hotel_id
        self.room_number = room_number
        self.room_type = room_type  # e.g., "standard", "deluxe", "suite"
        self.capacity = capacity  # max number of guests
        self.price_per_night = price_per_night
        self.beds = beds or []  # e.g., [{"type": "queen", "count": 1}, {"type": "single", "count": 1}]
        self.amenities = amenities or []
        self.accessibility_features = accessibility_features or []
        self.status = status  # "available", "occupied", "maintenance"
        self.images = images or []
        self.size_sqm = size_sqm
        self.created_at = datetime.utcnow()
        self.updated_at = self.created_at
        
    def to_dict(self):
        return {
            "hotel_id": self.hotel_id,
            "room_number": self.room_number,
            "room_type": self.room_type,
            "capacity": self.capacity,
            "price_per_night": self.price_per_night,
            "beds": self.beds,
            "amenities": self.amenities,
            "accessibility_features": self.accessibility_features,
            "status": self.status,
            "images": self.images,
            "size_sqm": self.size_sqm,
            "created_at": self.created_at,
            "updated_at": self.updated_at
        }