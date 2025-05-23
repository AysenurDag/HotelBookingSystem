import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './SearchBar.css';

const SearchBar = ({ initialValues = {}, onSearch }) => {
  const navigate = useNavigate();

  const getSessionDate = (key) => sessionStorage.getItem(key) || '';

  const [formValues, setFormValues] = useState({
    city: initialValues.city || '',
    country: initialValues.country || '',
    rating: initialValues.rating || '',
    check_in: getSessionDate('check_in') || '',
    check_out: getSessionDate('check_out') || '',
    guests: initialValues.guests || '',
    page: initialValues.page || '1',
    per_page: initialValues.per_page || '10'
  });

  useEffect(() => {
    setFormValues(prev => ({
      ...prev,
      check_in: getSessionDate('check_in'),
      check_out: getSessionDate('check_out')
    }));
  }, []);

  const handleChange = (e) => {
    const { name, value } = e.target;

    if (name === 'check_in' || name === 'check_out') {
      sessionStorage.setItem(name, value);
    }

    setFormValues(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    const queryParams = new URLSearchParams();
    Object.entries(formValues).forEach(([key, value]) => {
      if (value) queryParams.set(key, value);
    });

    navigate(`/results?${queryParams.toString()}`);

    if (onSearch) {
      onSearch(formValues);
    }
  };

  return (
    <form className="search-bar" onSubmit={handleSubmit}>
      <div className="first row">
        <div className="search-input city">
          <label htmlFor="city">City</label>
          <input
            type="text"
            id="city"
            name="city"
            value={formValues.city}
            onChange={handleChange}
            placeholder="e.g. Antalya"
            required
          />
        </div>

        <div className="search-input country">
          <label htmlFor="country">Country</label>
          <input
            type="text"
            id="country"
            name="country"
            value={formValues.country}
            onChange={handleChange}
            placeholder="e.g. Türkiye"
          />
        </div>

        <div className="search-input rating">
          <label htmlFor="rating">Rating</label>
          <select
            id="rating"
            name="rating"
            value={formValues.rating}
            onChange={handleChange}
          >
            <option value="">Any</option>
            {[1, 2, 3, 4, 5].map(r => (
              <option key={r} value={r}>{r} Stars</option>
            ))}
          </select>
        </div>
      </div>

      <div className="second row">
        <div className="search-input checkin">
          <label htmlFor="check_in">Check-in</label>
          <input
            type="date"
            id="check_in"
            name="check_in"
            value={formValues.check_in}
            onChange={handleChange}
          />
        </div>

        <div className="search-input checkout">
          <label htmlFor="check_out">Check-out</label>
          <input
            type="date"
            id="check_out"
            name="check_out"
            value={formValues.check_out}
            onChange={handleChange}
          />
        </div>

        <div className="search-input guests">
          <label htmlFor="guests">Guests</label>
          <select
            id="guests"
            name="guests"
            value={formValues.guests}
            onChange={handleChange}
          >
            <option value="">Any</option>
            {[1, 2, 3, 4, 5, 6, 7, 8].map(n => (
              <option key={n} value={n}>
                {n} {n === 1 ? 'Guest' : 'Guests'}
              </option>
            ))}
          </select>
        </div>
      </div>

      <button type="submit" className="search-button">Search</button>
    </form>
  );
};

export default SearchBar;
