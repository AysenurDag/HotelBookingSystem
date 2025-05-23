def setup_routes(api, blueprint):
    from api.controllers.hotel_controller import hotel_ns
    from api.controllers.room_controller import room_ns
    from api.controllers.room_availability_controller import room_availability_ns
    from flask import jsonify

    @blueprint.route('/health', methods=['GET'])
    def health_check():
        return jsonify({"status": "ok", "service": "hotel-service"})

    @blueprint.route('/', methods=['GET'])
    def check():
        return jsonify({"status": "ok"})

    api.add_namespace(hotel_ns, path='/hotels')
    api.add_namespace(room_ns, path='/rooms')
    api.add_namespace(room_availability_ns, path='/roomAvailability')
