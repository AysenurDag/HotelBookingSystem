import os
from dotenv import load_dotenv

load_dotenv()

class Config:
    DEBUG = False
    MONGO_URI = os.getenv('MONGODB_URI', 'mongodb://localhost:27017/hotel_db')

class DevelopmentConfig(Config):
    DEBUG = True

class TestingConfig(Config):
    DEBUG = True
    TESTING = True
    MONGO_URI = os.getenv('TEST_MONGODB_URI', 'mongodb://localhost:27017/test_hotel_db')

class ProductionConfig(Config):
    DEBUG = False

config = {
    'development': DevelopmentConfig,
    'testing': TestingConfig,
    'production': ProductionConfig,
    'default': DevelopmentConfig
}