// src/components/HotelCard/index.jsx
import React from 'react';
import { Link } from 'react-router-dom';
import './HotelCard.css';

const HotelCard = ({ hotel }) => {
  // Parse address from string to object
  let address = { street: '', city: '', country: '' };
  try {
    if (hotel.address) {
      // Remove single quotes and replace them with double quotes for valid JSON
      const cleanedAddress = hotel.address.replace(/'/g, '"');
      address = JSON.parse(cleanedAddress);
    }
  } catch (error) {
    console.error('Error parsing address:', error);
  }
  
  // Get the first image or use a placeholder
  const primaryImage = hotel.images && hotel.images.length > 0 
    ? hotel.images[0] 
    : '/placeholder-hotel.jpg';
  
  // Calculate price (this would come from your actual data in a real app)
  const price = Math.floor(Math.random() * 200) + 100; // Placeholder random price
  
  return (
    <div className="hotel-card">
      <div className="hotel-image">
        <img src={primaryImage} alt={hotel.name} onError={(e) => {
          e.target.onerror = null; 
          e.target.src = '/placeholder-hotel.jpg';
        }} />
      </div>
      
      <div className="hotel-info">
        <h3 className="hotel-name">{hotel.name}</h3>
        <div className="hotel-location">
          {address.city || hotel.city}, {address.country || hotel.country}
        </div>
        
        <div className="hotel-description">{hotel.description}</div>
        
        <div className="hotel-features">
          {hotel.amenities && hotel.amenities.slice(0, 3).map((amenity, index) => (
            <span key={index} className="feature-tag">{amenity}</span>
          ))}
          {hotel.amenities && hotel.amenities.length > 3 && (
            <span className="feature-tag">+{hotel.amenities.length - 3} more</span>
          )}
        </div>
        
        <div className="hotel-rating">
          <span className="rating-score">{hotel.rating}</span>
          <span className="rating-label">
            {hotel.rating >= 4.5 ? 'Excellent' : 
             hotel.rating >= 4.0 ? 'Very Good' : 
             hotel.rating >= 3.5 ? 'Good' : 'Average'}
          </span>
        </div>
      </div>
      
      <div className="hotel-price">
        <div className="price-amount">${price}</div>
        <div className="price-period">per night</div>
        <Link to={`/hotel/${hotel.id || 'detail'}`} className="view-details-btn">
          View Details
        </Link>
      </div>
    </div>
  );
};

export default HotelCard;