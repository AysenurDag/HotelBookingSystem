import React, { useCallback, useEffect, useRef } from 'react';
import { useParams } from 'react-router-dom';
import useRoomSearch from '../../hooks/useRoomSearch';
import RoomSearchBar from '../../components/RoomSearchBar';
import RoomCard from '../../components/RoomCard';

const HotelDetail = () => {
  const { hotelId } = useParams();
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
      handleSearch({
        check_in,
        check_out,
        page: 1,
      });

      hasInitialSearch.current = true;
    }
  }, [handleSearch]); // 💡 useCallback ile sabitlendiği için güvenli

  return (
    <div>
      <h2>Available Rooms in Hotel {hotelId}</h2>

      <RoomSearchBar onSearch={handleSearch} />

      {loading && <p>Loading...</p>}
      {error && <p>Error: {error}</p>}

      {rooms.length === 0 && !loading && <p>No rooms found.</p>}

      {rooms.map((room) => (
        <RoomCard key={room.id} room={room} />
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
