import { useState, useEffect } from 'react';
import { searchRooms } from '../services/hotelService'; 

const useRoomSearch = (initialParams = {}) => {
  const [searchParams, setSearchParams] = useState(initialParams);
  const [rooms, setRooms] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [pagination, setPagination] = useState({
    page: initialParams.page || 1,
    perPage: initialParams.perPage || 20,
    total: 0,
  });

  const fetchRooms = async () => {
    setLoading(true);
    setError(null);

    try {
      const data = await searchRooms(searchParams);

      setRooms(data.data || []);
      setPagination((prev) => ({
        ...prev,
        total: data.meta?.total || 0,
      }));
    } catch (err) {
      setError(err.message || 'Unknown error');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchRooms();
  }, [searchParams]);

  const updateSearch = (newParams) => {
    setSearchParams((prev) => ({
      ...prev,
      ...newParams,
    }));
  };

  const goToPage = (pageNumber) => {
    setSearchParams((prev) => ({
      ...prev,
      page: pageNumber,
    }));
  };

  return {
    rooms,
    loading,
    error,
    searchParams,
    pagination,
    updateSearch,
    goToPage,
  };
};

export default useRoomSearch;
