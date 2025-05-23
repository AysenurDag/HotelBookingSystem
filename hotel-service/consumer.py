import json
import logging
from bson import ObjectId
from datetime import datetime
from database import mongo  
                       
logging.basicConfig(level=logging.INFO)
def get_room_service():
    from services.room_availability_service import RoomAvailabilityService
    return RoomAvailabilityService

from flask import current_app
from factory import create_app

app = create_app()  # veya config ile: create_app("development")

def handle_message(ch, method, properties, body):
    with app.app_context():  # ✅ Flask context burada açılıyor
        try:
            message = json.loads(body)
            current_app.logger.info(f"[Hotel Consumer] Message received: {message}")
            booking_id = message.get("bookingId")
            check_in = message.get("checkInDate")
            check_out = message.get("checkOutDate")
            room_id = message.get("roomId")

            if not all([check_in, check_out, room_id]):
                raise ValueError("Eksik veri var: 'roomId', 'checkInDate', 'checkOutDate' zorunludur.")

            from services.room_availability_service import RoomAvailabilityService
            from database import mongo

            service = RoomAvailabilityService(mongo.db)
            availability_payload = {
                "bookingId": booking_id, 
                "roomId": room_id,
                "start_date": check_in,
                "end_date": check_out,
                "isAvailable": -1
            }

            result = service.create_availability(availability_payload)
            current_app.logger.info(f"[Hotel Consumer] Availability created: {result}")

            ch.basic_ack(delivery_tag=method.delivery_tag)

        except Exception as e:
            current_app.logger.error(f"[Hotel Consumer] Failed to process message: {e}")
            ch.basic_nack(delivery_tag=method.delivery_tag, requeue=False)

def handle_cancellation_message(ch, method, properties, body):
    with app.app_context():  # ✅ Flask context burada açılıyor
        try:
            message = json.loads(body)
            current_app.logger.info(f"[Hotel Consumer] Cancellation message received: {message}")

            booking_id = message.get("bookingId")
            check_in = message.get("checkInDate")
            check_out = message.get("checkOutDate")
            room_id = message.get("roomId")

            from services.room_availability_service import RoomAvailabilityService
            from database import mongo

            service = RoomAvailabilityService(mongo.db)

            availability_payload = {
                "bookingId": booking_id, 
                "roomId": room_id,
                "start_date": check_in,
                "end_date": check_out,
                "isAvailable": 1
            }
            
            result = service.create_availability(availability_payload)
            current_app.logger.info(f"[Hotel Consumer] Availability updated (booking canceled): {result}")

            ch.basic_ack(delivery_tag=method.delivery_tag)

        except Exception as e:
            current_app.logger.error(f"[Hotel Consumer] Failed to process cancellation message: {e}")
            ch.basic_nack(delivery_tag=method.delivery_tag, requeue=False)

