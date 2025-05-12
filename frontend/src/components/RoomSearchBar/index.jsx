import React, { useState, useEffect } from 'react';
import './RoomSearchBar.css';

const RoomSearchBar = ({ onSearch }) => {
  const [filters, setFilters] = useState({
    type: '',
    min_price: '',
    max_price: '',
    capacity: '',
    check_in: '',
    check_out: ''
  });

  useEffect(() => {
    const storedCheckIn = sessionStorage.getItem('check_in') || '';
    const storedCheckOut = sessionStorage.getItem('check_out') || '';

    const updated = {
      type: '',
      min_price: '',
      max_price: '',
      capacity: '',
      check_in: storedCheckIn,
      check_out: storedCheckOut
    };

    setFilters(updated);

    if (onSearch && (storedCheckIn || storedCheckOut)) {
      onSearch({
        ...updated,
        min_price: updated.min_price ? Number(updated.min_price) : undefined,
        max_price: updated.max_price ? Number(updated.max_price) : undefined,
        capacity: updated.capacity ? Number(updated.capacity) : undefined,
        page: 1
      });
    }
  }, []);

  
  const handleChange = (e) => {
    const { name, value } = e.target;

    // Tarih alanı değiştiğinde sessionStorage'a yaz
    if (name === 'check_in' || name === 'check_out') {
      sessionStorage.setItem(name, value);
    }

    setFilters((prev) => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    const searchParams = {
      ...filters,
      min_price: filters.min_price ? Number(filters.min_price) : undefined,
      max_price: filters.max_price ? Number(filters.max_price) : undefined,
      capacity: filters.capacity ? Number(filters.capacity) : undefined,
      page: 1,
    };

    onSearch(searchParams);
  };

  return (
    <form className="search-bar" onSubmit={handleSubmit}>
      <div className="first row">
        <div className="search-input">
          <label htmlFor="type">Room Type</label>
          <select name="type" id="type" value={filters.type} onChange={handleChange}>
            <option value="">Any</option>
            <option value="single">Single</option>
            <option value="double">Double</option>
            <option value="twin">Twin</option>
            <option value="suite">Suite</option>
            <option value="deluxe">Deluxe</option>
          </select>
        </div>

        <div className="search-input">
          <label htmlFor="capacity">Capacity</label>
          <input
            type="number"
            name="capacity"
            id="capacity"
            value={filters.capacity}
            onChange={handleChange}
            placeholder="e.g. 2"
          />
        </div>
      </div>

      <div className="second row">
        <div className="search-input">
          <label htmlFor="min_price">Min Price</label>
          <input
            type="number"
            name="min_price"
            id="min_price"
            value={filters.min_price}
            onChange={handleChange}
            placeholder="e.g. 50"
          />
        </div>

        <div className="search-input">
          <label htmlFor="max_price">Max Price</label>
          <input
            type="number"
            name="max_price"
            id="max_price"
            value={filters.max_price}
            onChange={handleChange}
            placeholder="e.g. 300"
          />
        </div>
      </div>

      <div className="second row">
        <div className="search-input">
          <label htmlFor="check_in">Check-in Date</label>
          <input
            type="date"
            name="check_in"
            id="check_in"
            value={filters.check_in}
            onChange={handleChange}
          />
        </div>

        <div className="search-input">
          <label htmlFor="check_out">Check-out Date</label>
          <input
            type="date"
            name="check_out"
            id="check_out"
            value={filters.check_out}
            onChange={handleChange}
          />
        </div>
      </div>

      <button type="submit" className="search-button">
        Search
      </button>
    </form>
  );
};

export default RoomSearchBar;
