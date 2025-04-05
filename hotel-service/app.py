from flask import Flask, jsonify
from flask_pymongo import PyMongo
from dotenv import load_dotenv
import os

# Load environment variables
load_dotenv()

app = Flask(__name__)
app.config["MONGO_URI"] = os.getenv("MONGODB_URI")
mongo = PyMongo(app)

@app.route('/api/health', methods=['GET'])
def health_check():
    return jsonify({"status": "ok", "service": "hotel-microservice"})

@app.route('/api/hotels', methods=['GET'])
def get_hotels():
    hotels = list(mongo.db.hotels.find({}, {'_id': 0}))
    return jsonify(hotels)

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True)