import React, { useCallback, useEffect, useRef, useState } from 'react';
import { useParams } from 'react-router-dom';
import useRoomSearch from '../../hooks/useRoomSearch';
import RoomSearchBar from '../../components/RoomSearchBar';
import RoomCard from '../../components/RoomCard';
import { createBooking } from '../../services/bookingService';

const HotelDetail = () => {
  const { hotelId } = useParams();
  const [selectedRoom, setSelectedRoom] = useState(null);
  const hasInitialSearch = useRef(false);

  const {
    rooms,
    loading,
    error,
    pagination,
    updateSearch,
    goToPage,
  } = useRoomSearch({
    hotel_id: hotelId,
    page: 1,
    perPage: 20,
  });

  const handleSearch = useCallback((filters) => {
    updateSearch({
      hotel_id: hotelId,
      ...filters,
    });
  }, [hotelId, updateSearch]);

  useEffect(() => {
    if (hasInitialSearch.current) return;

    const check_in = sessionStorage.getItem("check_in") || '';
    const check_out = sessionStorage.getItem("check_out") || '';

    if (check_in || check_out) {
      handleSearch({ check_in, check_out, page: 1 });
      hasInitialSearch.current = true;
    }
  }, [handleSearch]);

  const handleRoomSelect = (room) => {
    setSelectedRoom(room);
  };

  const handleCreateBooking = async () => {
    if (!selectedRoom) return;

    const check_in = sessionStorage.getItem("check_in");
    const check_out = sessionStorage.getItem("check_out");

    const bookingData = {
      roomId: selectedRoom.id,
      userId: "USER-456", // şimdilik hardcoded, login varsa değiştir
      checkInDate: check_in,
      checkOutDate: check_out,
      amount: selectedRoom.price_per_night,
      currency: "USD",
      status: "PENDING"
    };

    try {
      const result = await createBooking(bookingData);
      alert("✅ Booking successful!");
      console.log("Booking result:", result);
    } catch (err) {
      alert("❌ Booking failed: " + err.message);
    }
  };

  return (
    <div>
      <h2>Available Rooms in Hotel {hotelId}</h2>

      <RoomSearchBar onSearch={handleSearch} />

      {loading && <p>Loading...</p>}
      {error && <p>Error: {error}</p>}
      {rooms.length === 0 && !loading && <p>No rooms found.</p>}

      {rooms.map((room) => (
        <RoomCard
          key={room.id}
          room={room}
          isSelected={selectedRoom?.id === room.id}
          onSelect={handleRoomSelect}
        />
      ))}

      <div style={{ marginTop: '1rem' }}>
        <button
          onClick={handleCreateBooking}
          disabled={!selectedRoom}
          className="create-booking-button"
        >
          Create Booking
        </button>
      </div>
    </div>
  );
};

export default HotelDetail;
