import React from 'react';
import './RoomCard.css'; // Opsiyonel stil dosyası

const RoomCard = ({ room }) => {
  return (
    <div className="room-card">
      <h3>Room {room.room_number} - {room.type.toUpperCase()}</h3>
      <p><strong>Capacity:</strong> {room.capacity} person(s)</p>
      <p><strong>Price:</strong> ${room.price_per_night} per night</p>
      <p><strong>Description:</strong> {room.description}</p>

      {room.amenities?.length > 0 && (
        <p><strong>Amenities:</strong> {room.amenities.join(', ')}</p>
      )}

      {room.images?.length > 0 && (
        <img
          src={room.images[0]} // İlk resmi göster
          alt={`Room ${room.room_number}`}
          className="room-image"
        />
      )}

      <p style={{ color: room.is_active ? 'green' : 'red' }}>
        {room.is_active ? 'Available' : 'Not Available'}
      </p>
    </div>
  );
};

export default RoomCard;
