import React from 'react';
import './RoomCard.css';

const RoomCard = ({ room, isSelected, onSelect }) => {
  const handleClick = () => {
    if (onSelect) onSelect(room);
  };

  return (
    <div
      className={`room-card ${isSelected ? 'selected' : ''}`}
      onClick={handleClick}
      style={{ cursor: 'pointer' }}
    >
      <h3>Room {room.room_number} - {room.type.toUpperCase()}</h3>
      <p><strong>Capacity:</strong> {room.capacity} person(s)</p>
      <p><strong>Price:</strong> ${room.price_per_night} per night</p>
      <p><strong>Description:</strong> {room.description}</p>

      {room.amenities?.length > 0 && (
        <p><strong>Amenities:</strong> {room.amenities.join(', ')}</p>
      )}

      {room.images?.length > 0 && (
        <img
          src={room.images[0] || "images/antalya.jpg"}
          alt={`Room ${room.room_number}`}
          className="room-image"
        />
      )}
    </div>
  );
};

export default RoomCard;
