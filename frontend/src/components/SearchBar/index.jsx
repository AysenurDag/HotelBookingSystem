// src/components/SearchBar/index.jsx
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import './SearchBar.css';

const SearchBar = ({ initialValues, onSearch }) => {
  const navigate = useNavigate();
  const [formValues, setFormValues] = useState({
    destination: initialValues.destination || '',
    checkIn: initialValues.checkIn || '',
    checkOut: initialValues.checkOut || '',
    guests: initialValues.guests || '1'
  });

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormValues(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    
    // Create search query params
    const queryParams = new URLSearchParams();
    Object.entries(formValues).forEach(([key, value]) => {
      if (value) queryParams.set(key, value);
    });
    
    // Update URL
    navigate(`/search?${queryParams.toString()}`);
    
    // Call the onSearch callback
    if (onSearch) {
      onSearch(formValues);
    }
  };

  return (
    <form className="search-bar" onSubmit={handleSubmit}>
      <div className="search-input">
        <label htmlFor="destination">Destination</label>
        <input
          type="text"
          id="destination"
          name="destination"
          value={formValues.destination}
          onChange={handleChange}
          placeholder="Where are you going?"
          required
        />
      </div>
      
      <div className="search-input">
        <label htmlFor="checkIn">Check-in</label>
        <input
          type="date"
          id="checkIn"
          name="checkIn"
          value={formValues.checkIn}
          onChange={handleChange}
          required
        />
      </div>
      
      <div className="search-input">
        <label htmlFor="checkOut">Check-out</label>
        <input
          type="date"
          id="checkOut"
          name="checkOut"
          value={formValues.checkOut}
          onChange={handleChange}
          required
        />
      </div>
      
      <div className="search-input">
        <label htmlFor="guests">Guests</label>
        <select
          id="guests"
          name="guests"
          value={formValues.guests}
          onChange={handleChange}
        >
          {[1, 2, 3, 4, 5, 6].map(num => (
            <option key={num} value={num}>{num} {num === 1 ? 'Guest' : 'Guests'}</option>
          ))}
        </select>
      </div>
      
      <button type="submit" className="search-button">Search</button>
    </form>
  );
};

export default SearchBar;