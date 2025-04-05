from datetime import datetime

class Hotel:
    def __init__(self, name, location, rating=0, rooms=None):
        self.name = name
        self.location = location
        self.rating = rating
        self.rooms = rooms or []
        self.created_at = datetime.utcnow()
        
    def to_dict(self):
        return {
            "name": self.name,
            "location": self.location,
            "rating": self.rating,
            "rooms": self.rooms,
            "created_at": self.created_at
        }