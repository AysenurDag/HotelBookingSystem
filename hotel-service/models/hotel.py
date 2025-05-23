from flask_restx import fields

def get_hotel_model(api):
    return api.model('Hotel', {
        'id': fields.String(readonly=True, description='The hotel unique identifier'),
        'name': fields.String(required=True, description='Hotel name'),
        'address': fields.Nested(api.model('HotelAddress', {
            'city': fields.String(description='Hotel city'),
            'country': fields.String(description='Hotel country')
        }), description='Hotel address'),
        'rating': fields.Float(description='Hotel rating (1 to 5)', min=1, max=5),
        'amenities': fields.List(fields.String, description='List of hotel amenities'),
        'contact': fields.Nested(api.model('HotelContact', {
            'phone': fields.String(required=True, description='Hotel contact phone'),
            'email': fields.String(required=True, description='Hotel contact email')
        }), description='Hotel contact information'),
        'description': fields.String(description='Hotel description'),
        'images': fields.List(fields.String, description='List of hotel image URLs'),
    })

def get_pagination_model(api):
    return api.model('PaginationMeta', {
        'page': fields.Integer(description='Current page number'),
        'per_page': fields.Integer(description='Items per page'),
        'total': fields.Integer(description='Total number of items')
    })

def get_hotel_list_model(api):
    return api.model('HotelList', {
        'data': fields.List(fields.Nested(get_hotel_model(api)), description='List of hotels'),
        'meta': fields.Nested(get_pagination_model(api), description='Pagination metadata')
    })
