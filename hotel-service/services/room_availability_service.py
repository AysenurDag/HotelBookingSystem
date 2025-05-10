from flask import current_app
from bson.objectid import ObjectId
from datetime import datetime, timedelta, time
from database import mongo

class RoomAvailabilityService:
    def __init__(self, db=None):
        self._db = db or mongo.db

    def _handle_error(self, message, status=500):
        current_app.logger.error(message)
        return {"error": message, "status_code": status}

    def _serialize_availability(self, doc):
        return {
            "id": str(doc.get("_id")),
            "room_id": str(doc.get("roomId")),
            "date": doc.get("date").isoformat() if doc.get("date") else None,
            "isAvailable": doc.get("isAvailable")

        }

    def create_availability(self, data):
        current_app.logger.info(f"POST data: {data}")
        try:
            room_id = data.get("roomId")
            is_available = data.get("isAvailable")

            dates = data.get("dates", [])
            start_date = data.get("start_date")
            end_date = data.get("end_date")

            if not room_id or is_available is None:
                return self._handle_error("Missing required fields", 400)

            generated_dates = []

            if start_date and end_date:
                start = datetime.fromisoformat(start_date).date()
                end = datetime.fromisoformat(end_date).date()
                if end < start:
                    return self._handle_error("End date cannot be before start date", 400)
                delta = (end - start).days
                generated_dates = [start + timedelta(days=i) for i in range(delta + 1)]

            elif start_date:
                generated_dates = [datetime.fromisoformat(start_date).date()]

            elif dates:
                generated_dates = [datetime.fromisoformat(d).date() for d in dates]

            else:
                return self._handle_error("Provide at least one of: 'dates', 'start_date', or 'start_date + end_date'", 400)

            is_available = data.get("isAvailable")

            if is_available is None:
                return self._handle_error("Missing field: isAvailable", 400)
            # Her tarih için ayrı belge oluştur
            documents = [{
                "roomId": ObjectId(room_id),
                "date": datetime.combine(d, datetime.min.time()),
                "isAvailable": is_available
            } for d in generated_dates]

            result = self._db.availability.insert_many(documents)
             
            return {"inserted_ids": [str(rid) for rid in result.inserted_ids]}
        except Exception as e:
            return self._handle_error(f"Error creating availability: {str(e)}")



    def get_availability_by_room(self, room_id, start_date=None, end_date=None):
        try:
            query = {"roomId": ObjectId(room_id)}

            if start_date and end_date:
                start = datetime.combine(datetime.fromisoformat(start_date).date(), time.min)  # 00:00:00
                end = datetime.combine(datetime.fromisoformat(end_date).date(), time.max)      # 23:59:59.999999
                query["date"] = {"$gte": start, "$lte": end}

            current_app.logger.info(f"Querying availability with: {query}")

            docs = self._db.availability.find(query)
            return [self._serialize_availability(doc) for doc in docs]

        except Exception as e:
            return self._handle_error(f"Error fetching availability: {str(e)}")


    def update_availability(self, availability_id, update_data):
        try:
            if "date" in update_data:
                update_data["date"] = datetime.fromisoformat(update_data["date"])

            result = self._db.availability.update_one(
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
            result = self._db.availability.delete_one({"_id": ObjectId(availability_id)})
            if result.deleted_count == 0:
                return self._handle_error("Availability entry not found", 404)
            return {"success": True}
        except Exception as e:
            return self._handle_error(f"Error deleting availability: {str(e)}")
