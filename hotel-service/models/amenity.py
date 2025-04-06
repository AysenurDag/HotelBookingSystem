class Amenity:
    def __init__(self, name, description=None, category=None, icon=None):
        self.name = name
        self.description = description
        self.category = category  # e.g., "general", "room", "accessibility"
        self.icon = icon  # e.g., "wifi", "parking", "wheelchair"
        
    def to_dict(self):
        return {
            "name": self.name,
            "description": self.description,
            "category": self.category,
            "icon": self.icon
        }