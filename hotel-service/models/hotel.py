from datetime import datetime

class Hotel:
    def __init__(self, name, address, city, country, description, rating=0, 
                 amenities=None, accessibility_features=None, images=None, 
                 contact_info=None, check_in_time="14:00", check_out_time="12:00"):
        self.name = name
        self.address = address
        self.city = city
        self.country = country
        self.description = description
        self.rating = rating
        self.amenities = amenities or []
        self.accessibility_features = accessibility_features or []
        self.images = images or []
        self.contact_info = contact_info or {}
        self.check_in_time = check_in_time
        self.check_out_time = check_out_time
        self.created_at = datetime.utcnow()
        self.updated_at = self.created_at
        
    def to_dict(self):
        return {
            "name": self.name,
            "address": self.address,
            "city": self.city,
            "country": self.country,
            "description": self.description,
            "rating": self.rating,
            "amenities": self.amenities,
            "accessibility_features": self.accessibility_features,
            "images": self.images,
            "contact_info": self.contact_info,
            "check_in_time": self.check_in_time,
            "check_out_time": self.check_out_time,
            "created_at": self.created_at,
            "updated_at": self.updated_at
        }