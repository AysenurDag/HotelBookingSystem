from flask_restx import fields

def get_room_model(api):
    return api.model('Room', {
        'id': fields.String(readonly=True, description='Room identifier'),
        'hotelId': fields.String(required=True, description='Hotel identifier'),  
        'roomNumber': fields.String(required=True, description='Room number or identifier within the hotel'),
        'type': fields.String(required=True, description='Type of room (single, double, twin, suite, deluxe)'),
        'capacity': fields.Integer(required=True, description='Maximum number of guests allowed'),
        'pricePerNight': fields.Float(required=True, description='Standard price per night'),
        'amenities': fields.List(fields.String, description='List of amenities in the room'),
        'description': fields.String(description='Room description'),
        'images': fields.List(fields.String, description='URLs to room images'),
    })
def get_room_list_model(api):
    pagination_model = api.model('RoomPaginationMeta', {
        'page': fields.Integer(description='Current page number'),
        'per_page': fields.Integer(description='Items per page'),
        'total': fields.Integer(description='Total number of items'),
        'hotelId': fields.String(description='Hotel identifier for the rooms')
    })

    return api.model('RoomList', {
        'data': fields.List(fields.Nested(get_room_model(api)), description='List of rooms'),
        'meta': fields.Nested(pagination_model, description='Pagination metadata')
    })
