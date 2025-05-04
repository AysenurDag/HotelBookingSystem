// src/pages/results/index.jsx
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
    perPage: parseInt(queryParams.get('perPage') || '10', 10)
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
    // Create updated URL query string
    const newQueryParams = new URLSearchParams();
    Object.entries(newSearchValues).forEach(([key, value]) => {
      if (value) newQueryParams.set(key, value);
    });
    
    // Update URL
    navigate(`/search?${newQueryParams.toString()}`);
    
    // Update search parameters in our hook
    updateSearch(newSearchValues);
  };

  // Calculate total pages
  const totalPages = Math.ceil(pagination.total / pagination.perPage);
  
  return (
    <div className="search-results-pages">
      <div className="search-bar-container">
        <SearchBar initialValues={searchParams} onSearch={handleSearch} />
      </div>
      
      <div className="results-container">
        <h2>
          {searchParams.destination 
            ? `Hotels in ${searchParams.destination}` 
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
              {hotels.map((hotel, index) => (
                <HotelCard key={index} hotel={hotel} />
              ))}
            </div>
            
            {/* Pagination controls */}
            {totalPages > 1 && (
              <div className="pagination">
                <button 
                  onClick={() => goToPage(pagination.page - 1)}
                  disabled={pagination.page === 1}
                  className="page-button"
                >
                  Previous
                </button>
                
                <div className="page-info">
                  Page {pagination.page} of {totalPages}
                </div>
                
                <button 
                  onClick={() => goToPage(pagination.page + 1)}
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