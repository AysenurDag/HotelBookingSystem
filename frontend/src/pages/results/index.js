import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import SearchBar from '../../components/SearchBar';
import HotelCard from '../../components/HotelCard';
import useHotelSearch from '../../hooks/useHotelSearch';
import './results.css';

const SearchResults = () => {
  const location = useLocation();
  const navigate = useNavigate();

  // Extract search parameters from URL
  const queryParams = new URLSearchParams(location.search);
  const initialSearchParams = {
    page: parseInt(queryParams.get('page') || '1', 10),
    perPage: parseInt(queryParams.get('perPage') || '10', 10),
    city: queryParams.get('city') || '',
    country: queryParams.get('country') || '',
    rating: queryParams.get('rating') ? parseInt(queryParams.get('rating'), 10) : '',
    amenities: queryParams.getAll('amenities') || []
  };

  // Use our custom hook
  const {
    searchParams,
    updateSearch,
    hotels,
    loading,
    error,
    pagination,
    goToPage
  } = useHotelSearch(initialSearchParams);

  // Handle search form submission
  const handleSearch = (newSearchValues) => {
    const newQueryParams = new URLSearchParams();
    Object.entries({ ...newSearchValues, page: 1 }).forEach(([key, value]) => {
      if (Array.isArray(value)) {
        value.forEach((item) => newQueryParams.append(key, item));
      } else if (value !== '') {
        newQueryParams.set(key, value);
      }
    });

    navigate(`/search?${newQueryParams.toString()}`);
    updateSearch({ ...newSearchValues, page: 1 });
  };

  // Handle pagination with URL update
  const handlePageChange = (page) => {
    const updatedParams = { ...searchParams, page };
    const query = new URLSearchParams();

    Object.entries(updatedParams).forEach(([key, value]) => {
      if (Array.isArray(value)) {
        value.forEach((v) => query.append(key, v));
      } else {
        query.set(key, value);
      }
    });

    navigate(`/search?${query.toString()}`);
    goToPage(page);
  };

  const totalPages = Math.ceil(pagination.total / pagination.perPage);

  return (
    <div className="search-results-pages">
      <div className="search-bar-container">
        <SearchBar initialValues={searchParams} onSearch={handleSearch} />
      </div>

      <div className="results-container">
        <h2>
          {searchParams.city
            ? `Hotels in ${searchParams.city}`
            : 'All Available Hotels'}
        </h2>

        {error && (
          <div className="error-message">
            Error: {error}. Please try again.
          </div>
        )}

        {loading ? (
          <div className="loading">Loading hotels...</div>
        ) : hotels.length > 0 ? (
          <>
            <div className="hotel-list">
              {hotels.map((hotel) => (
                <HotelCard key={hotel._id} hotel={hotel} />
              ))}
            </div>

            {totalPages > 1 && (
              <div className="pagination">
                <button
                  onClick={() => handlePageChange(pagination.page - 1)}
                  disabled={pagination.page === 1}
                  className="page-button"
                >
                  Previous
                </button>

                <div className="page-info">
                  Page {pagination.page} of {totalPages}
                </div>

                <button
                  onClick={() => handlePageChange(pagination.page + 1)}
                  disabled={pagination.page === totalPages}
                  className="page-button"
                >
                  Next
                </button>
              </div>
            )}
          </>
        ) : (
          <div className="no-results">
            No hotels found for your search criteria. Try adjusting your search.
          </div>
        )}
      </div>
    </div>
  );
};

export default SearchResults;
