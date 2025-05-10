import React from 'react';
import { useParams } from 'react-router-dom';
import useRoomSearch from '../../hooks/useRoomSearch';
import RoomSearchBar from '../../components/RoomSearchBar';
import RoomCard from '../../components/RoomCard';

const HotelDetail = () => {
  const { hotelId } = useParams();

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

  const handleSearch = (filters) => {
    updateSearch({
      hotel_id: hotelId,
      ...filters,
    });
  };

  return (
    <div>
      <h2>Available Rooms</h2>

      <RoomSearchBar onSearch={handleSearch} />

      {loading && <p>Loading...</p>}
      {error && <p>Error: {error}</p>}

      {rooms.map((room) => (
        <RoomCard key={room.room_number} room={room} />
      ))}

      <div>
        <button
          onClick={() => goToPage(pagination.page - 1)}
          disabled={pagination.page === 1}
        >
          Previous
        </button>
        <span> Page {pagination.page} </span>
        <button
          onClick={() => goToPage(pagination.page + 1)}
          disabled={rooms.length < pagination.perPage}
        >
          Next
        </button>
      </div>
    </div>
  );
};

export default HotelDetail;
