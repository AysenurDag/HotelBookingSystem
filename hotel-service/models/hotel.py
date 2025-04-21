from flask_restx import fields
from datetime import datetime

def get_hotel_model(api):
    """
    Define the Hotel model for Swagger documentation
    """
    return api.model('Hotel', {
        'id': fields.String(readonly=True, description='The hotel unique identifier'),
        'name': fields.String(required=True, description='Hotel name'),
        'address': fields.String(required=True, description='Hotel address'),
        'city': fields.String(required=True, description='Hotel city'),
        'country': fields.String(required=True, description='Hotel country'),
        'rating': fields.Float(description='Hotel rating', min=1, max=5),
        'email': fields.String(description='Hotel contact email'),
        'phone': fields.String(description='Hotel contact phone'),
        'description': fields.String(description='Hotel description'),
        'amenities': fields.List(fields.String, description='List of hotel amenities'),
        'images': fields.List(fields.String, description='List of hotel image URLs'),
        'created_at': fields.DateTime(readonly=True, description='Creation timestamp'),
        'updated_at': fields.DateTime(readonly=True, description='Last update timestamp')
    })

"""
bu ne
"""
def get_pagination_model(api):
    """
    Define the Pagination metadata model for Swagger documentation
    """
    return api.model('PaginationMeta', {
        'page': fields.Integer(description='Current page number'),
        'per_page': fields.Integer(description='Items per page'),
        'total': fields.Integer(description='Total number of items')
    })

def get_hotel_list_model(api):
    """
    Define the Hotel list model with pagination for Swagger documentation
    """
    return api.model('HotelList', {
        'data': fields.List(fields.Nested(get_hotel_model(api)), description='List of hotels'),
        'meta': fields.Nested(get_pagination_model(api), description='Pagination metadata')
    })