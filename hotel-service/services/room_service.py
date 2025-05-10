from flask import current_app
from bson.objectid import ObjectId
from database import mongo

class RoomService:
    def __init__(self, db=None):
        self._db = db or mongo.db

    @property
    def db(self):
        return self._db

    def _serialize_room(self, room):
        return {
            "id": str(room["_id"]),
            "hotel_id": str(room.get("hotel_id")) if room.get("hotel_id") else None,
            "room_number": room.get("room_number"),
            "type": room.get("type"),
            "capacity": room.get("capacity"),
            "price_per_night": room.get("price_per_night"),
            "description": room.get("description", ""),
            "amenities": room.get("amenities", []),
            "images": room.get("images", []),
            "is_active": room.get("is_active")
        }

    def _handle_error(self, message, status=500):
        current_app.logger.error(message)
        return {"error": message, "status_code": status}

    def get_rooms(self, filters=None, page=1, per_page=20):
        filters = filters or {}
        query = {}

        if "hotel_id" in filters:
            query["hotel_id"] = ObjectId(filters["hotel_id"])

        if "type" in filters:
            query["type"] = filters["type"]

        if "min_price" in filters or "max_price" in filters:
            query["price_per_night"] = {}
            if "min_price" in filters:
                query["price_per_night"]["$gte"] = filters["min_price"]
            if "max_price" in filters:
                query["price_per_night"]["$lte"] = filters["max_price"]

        if "capacity" in filters:
            query["capacity"] = {"$gte": filters["capacity"]}

        if filters.get("status") == "active":
            query["is_active"] = True
        elif filters.get("status") == "inactive":
            query["is_active"] = False

        skip = (page - 1) * per_page
        total = self.db.rooms.count_documents(query)
        rooms = list(self.db.rooms.find(query).skip(skip).limit(per_page))

        return [self._serialize_room(r) for r in rooms], total

    def get_room_by_id(self, room_id):
        try:
            room = self.db.rooms.find_one({"_id": ObjectId(room_id)})
            return self._serialize_room(room)
        except Exception as e:
            return self._handle_error(f"Error getting room: {str(e)}")

    def create_room(self, room_data):
        try:
            hotel_id = room_data.get("hotel_id")
            if not hotel_id:
                return self._handle_error("Hotel ID is required", 400)

            # Check if hotel exists
            hotel = self.db.hotels.find_one({"_id": ObjectId(hotel_id)})
            if not hotel:
                return self._handle_error("Hotel not found", 404)

            # Check uniqueness
            existing = self.db.rooms.find_one({
                "hotel_id": ObjectId(hotel_id),
                "room_number": room_data["room_number"]
            })
            if existing:
                return self._handle_error("Room with this number already exists in this hotel", 409)

            room_data["hotel_id"] = ObjectId(hotel_id)
            room_data["is_active"] = True  # default
            result = self.db.rooms.insert_one(room_data)
            return {"id": str(result.inserted_id)}
        except Exception as e:
            return self._handle_error(f"Error creating room: {str(e)}")

    def update_room(self, room_id, room_data):
        try:
            if "hotel_id" in room_data:
                room_data["hotel_id"] = ObjectId(room_data["hotel_id"])

            result = self.db.rooms.update_one(
                {"_id": ObjectId(room_id)},
                {"$set": room_data}
            )

            if result.matched_count == 0:
                return self._handle_error("Room not found", 404)

            return {"success": True}
        except Exception as e:
            return self._handle_error(f"Error updating room: {str(e)}")

    def delete_room(self, room_id):
        try:
            result = self.db.rooms.update_one(
                {"_id": ObjectId(room_id)},
                {"$set": {"is_active": False}}
            )
            if result.matched_count == 0:
                return self._handle_error("Room not found", 404)
            return {"success": True}
        except Exception as e:
            return self._handle_error(f"Error deleting room: {str(e)}")
