from flask import current_app
from bson.objectid import ObjectId
from datetime import datetime
from database import mongo

 
class RoomAvailabilityService:
    def __init__(self, db=None):
        self._db = db or mongo.db

    def _handle_error(self, message, status=500):
        current_app.logger.error(message)
        return {"error": message, "status_code": status}

    def _serialize_availability(self, doc):
        return {
            "id": str(doc["_id"]),
            "room_id": str(doc["roomId"]),
            "dates": [d.isoformat() for d in doc.get("dates", [])],
            "is_available": doc.get("isAvailable")
        }

    def create_availability(self, data):
        try:
            room_id = data.get("roomId")
            dates = data.get("dates")
            is_available = data.get("isAvailable")

            if not room_id or not dates or is_available is None:
                return self._handle_error("Missing required fields", 400)

            availability_doc = {
                "roomId": ObjectId(room_id),
                "dates": [datetime.fromisoformat(d) for d in dates],
                "isAvailable": is_available
            }

            result = self._db.room_availability.insert_one(availability_doc)
            return {"id": str(result.inserted_id)}
        except Exception as e:
            return self._handle_error(f"Error creating availability: {str(e)}")

    def get_availability_by_room(self, room_id):
        try:
            docs = self._db.room_availability.find({"roomId": ObjectId(room_id)})
            return [self._serialize_availability(doc) for doc in docs]
        except Exception as e:
            return self._handle_error(f"Error fetching availability: {str(e)}")

    def update_availability(self, availability_id, update_data):
        try:
            if "dates" in update_data:
                update_data["dates"] = [datetime.fromisoformat(d) for d in update_data["dates"]]

            result = self._db.room_availability.update_one(
                {"_id": ObjectId(availability_id)},
                {"$set": update_data}
            )

            if result.matched_count == 0:
                return self._handle_error("Availability entry not found", 404)

            return {"success": True}
        except Exception as e:
            return self._handle_error(f"Error updating availability: {str(e)}")

    def delete_availability(self, availability_id):
        try:
            result = self._db.room_availability.delete_one({"_id": ObjectId(availability_id)})
            if result.deleted_count == 0:
                return self._handle_error("Availability entry not found", 404)
            return {"success": True}
        except Exception as e:
            return self._handle_error(f"Error deleting availability: {str(e)}")
