// src/pages/HotelDetail.jsx
import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import axios from 'axios';

const HotelDetail = () => {
  const { id } = useParams(); // /hotel/:id
  const [rooms, setRooms] = useState([]);
  const [hotel, setHotel] = useState(null);

  useEffect(() => {
    const fetchHotelAndRooms = async () => {
      try {
        // Otel bilgisi
        const hotelRes = await axios.get(`/api/hotels/${id}`);
        setHotel(hotelRes.data);

        // Oda bilgisi
        const roomRes = await axios.get(`/api/rooms?hotel_id=${id}`);
        setRooms(roomRes.data.data); // API response -> { data: rooms, meta: { ... } }
      } catch (err) {
        console.error('Error fetching hotel or rooms:', err);
      }
    };

    fetchHotelAndRooms();
  }, [id]);

  if (!hotel) return <div>Loading...</div>;

  return (
    <div className="hotel-detail-page">
      <h1>{hotel.name}</h1>
      <p>{hotel.description}</p>

      <h2>Rooms</h2>
      {rooms.length === 0 ? (
        <p>No rooms available for this hotel.</p>
      ) : (
        <ul>
          {rooms.map(room => (
            <li key={room.id}>
              <strong>{room.room_number}</strong> – {room.type} – ${room.price_per_night}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
};

export default HotelDetail;
