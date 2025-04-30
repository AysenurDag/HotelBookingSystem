// src/hooks/useHotelSearch.js
import { useState, useEffect } from 'react';
import { searchHotels } from '../services/hotelService';

const useHotelSearch = (initialSearchParams) => {
  const [searchParams, setSearchParams] = useState({
    ...initialSearchParams,
    page: 1,
    perPage: 10
  });
  const [hotels, setHotels] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [pagination, setPagination] = useState({
    page: 1,
    perPage: 10,
    total: 0
  });

  // Function to update search parameters
  const updateSearch = (newParams) => {
    // Reset to page 1 when search criteria changes
    if (Object.keys(newParams).some(key => key !== 'page' && key !== 'perPage')) {
      setSearchParams(prev => ({ ...prev, ...newParams, page: 1 }));
    } else {
      setSearchParams(prev => ({ ...prev, ...newParams }));
    }
  };

  // Function to handle pagination
  const goToPage = (page) => {
    updateSearch({ page });
  };

  // Fetch hotels when search parameters change
  useEffect(() => {
    const fetchHotels = async () => {
      setLoading(true);
      setError(null);
      
      try {
        const response = await searchHotels(searchParams);
        
        // Extract data and pagination info from the response
        const { data, meta } = response;
        
        setHotels(data || []);
        setPagination({
          page: meta?.page || 1,
          perPage: meta?.per_page || 10,
          total: meta?.total || 0
        });
      } catch (err) {
        setError(err.message || 'Failed to fetch hotels');
        setHotels([]);
      } finally {
        setLoading(false);
      }
    };

    fetchHotels();
  }, [searchParams]);

  return {
    searchParams,
    updateSearch,
    hotels,
    loading,
    error,
    pagination,
    goToPage
  };
};

export default useHotelSearch;