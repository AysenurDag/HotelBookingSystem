import { useState, useEffect } from 'react';
import { searchHotels } from '../services/hotelService';

const useHotelSearch = (initialSearchParams) => {
  const [searchParams, setSearchParams] = useState({
    city: initialSearchParams.city || '',
    country: initialSearchParams.country || '',
    rating: initialSearchParams.rating || '',
    amenities: initialSearchParams.amenities || [],
    page: initialSearchParams.page || 1,
    per_page: initialSearchParams.per_page || 10,
  });

  const [hotels, setHotels] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [pagination, setPagination] = useState({
    page: searchParams.page,
    per_page: searchParams.per_page,
    total: 0,
  });

  const updateSearch = (newParams) => {
    const resetPage = Object.keys(newParams).some(
      (key) => key !== 'page' && key !== 'per_page'
    );

    setSearchParams((prev) => ({
      ...prev,
      ...newParams,
      page: resetPage ? 1 : newParams.page || prev.page,
    }));
  };

  const goToPage = (page) => {
    updateSearch({ page });
  };

    const fetchHotels = async () => {
  setLoading(true);
  setError(null);

  try {
    const result = await searchHotels(searchParams);

    const fetchedHotels = result.data || [];
    const meta = result.meta || {};
    const total = meta.total || 0;

    setHotels(fetchedHotels);
    setPagination({
      page: meta.page || searchParams.page,
      per_page: meta.per_page || searchParams.per_page,
      total
    });
  } catch (err) {
    setError(err.message || 'Failed to fetch hotels');
    setHotels([]);
  } finally {
    setLoading(false);
  }
};

  useEffect(() => {
    fetchHotels();
  }, [searchParams]);
  
  useEffect(() => {
    const intervalId = setInterval(() => {
      fetchHotels(); // düzenli olarak çağır
    }, 30000);

    return () => clearInterval(intervalId); // sadece 1 kez kurulur
  }, []); // sadece sayfa yüklendiğinde kurulsun
  return {
    searchParams,
    updateSearch,
    hotels,
    loading,
    error,
    pagination,
    goToPage,
  };
};

export default useHotelSearch;
