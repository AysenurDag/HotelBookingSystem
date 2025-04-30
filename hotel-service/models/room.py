from flask_restx import fields

def get_room_model(api):
    """
    Define the Room model for Swagger documentation
    """
    return api.model('Room', {
        'id': fields.String(readonly=True, description='Room identifier'),
        'hotel_id': fields.String(required=True, description='Hotel identifier'),
        'room_type': fields.String(required=True, description='Type of room'),
        'name': fields.String(required=True, description='Name of the room'),
        'description': fields.String(description='Room description'),
        'price': fields.Float(required=True, description='Room price per night'),
        'capacity': fields.Integer(required=True, description='Room capacity'),
        'amenities': fields.List(fields.String, description='List of room amenities'),
        'images': fields.List(fields.String, description='List of room image URLs'),
        'status': fields.String(description='Room availability status'),
        'created_at': fields.DateTime(readonly=True, description='Creation timestamp'),
        'updated_at': fields.DateTime(readonly=True, description='Last update timestamp')
    })

def get_room_list_model(api):
    """
    Define the Room list model with pagination for Swagger documentation
    """
    pagination_model = api.model('RoomPaginationMeta', {
        'page': fields.Integer(description='Current page number'),
        'per_page': fields.Integer(description='Items per page'),
        'total': fields.Integer(description='Total number of items'),
        'hotel_id': fields.String(description='Hotel identifier for the rooms')
    })
    
    return api.model('RoomList', {
        'data': fields.List(fields.Nested(get_room_model(api)), description='List of rooms'),
        'meta': fields.Nested(pagination_model, description='Pagination metadata')
    })