// src/services/hotelService.js
const API_URL = process.env.REACT_APP_API_KEY;

export const searchHotels = async (searchParams) => {
  try {
    const queryParams = new URLSearchParams();

    if (searchParams.city) {
      queryParams.set('city', searchParams.city);
    }

    if (searchParams.country) {
      queryParams.set('country', searchParams.country);
    }

    if (searchParams.rating) {
      queryParams.set('rating', searchParams.rating);
    }

    if (searchParams.amenities && searchParams.amenities.length > 0) {
      // Flask backend expects amenities as an array, so send each as separate
      for (const amenity of searchParams.amenities) {
        queryParams.append('amenities', amenity);
      }
    }

    queryParams.set('page', searchParams.page || 1);
    queryParams.set('per_page', searchParams.perPage || 10);

    console.log(`Making request to: ${API_URL}/hotels/getHotels?${queryParams.toString()}`);

    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), 20000); // 20 second timeout

    const response = await fetch(`${API_URL}/hotels/getHotels?${queryParams.toString()}`, {
      signal: controller.signal,
      mode: 'cors',
      headers: {
        'Accept': 'application/json',
      },
    });

    clearTimeout(timeoutId);

    if (!response.ok) {
      throw new Error(`Error ${response.status}: ${response.statusText}`);
    }

    return await response.json();
  } catch (error) {
    if (error.name === 'AbortError') {
      console.error('Request timed out');
      throw new Error('The request timed out. The server might be busy or unavailable.');
    }
    console.error('Error searching hotels:', error);
    throw error;
  }
};

export const searchRooms = async (searchParams) => {
  const API_URL = process.env.REACT_APP_API_KEY;

  try {
    const queryParams = new URLSearchParams();

    if (searchParams.hotel_id) {
      queryParams.set('hotel_id', searchParams.hotel_id);
    }

    if (searchParams.type) {
      queryParams.set('type', searchParams.type);
    }

    if (searchParams.min_price) {
      queryParams.set('min_price', searchParams.min_price);
    }

    if (searchParams.max_price) {
      queryParams.set('max_price', searchParams.max_price);
    }

    if (searchParams.capacity) {
      queryParams.set('capacity', searchParams.capacity);
    }

    if (searchParams.check_in) {
      queryParams.set('check_in', searchParams.check_in);
    }

    if (searchParams.check_out) {
      queryParams.set('check_out', searchParams.check_out);
    }

    queryParams.set('page', searchParams.page || 1);
    queryParams.set('per_page', searchParams.perPage || 20);

    const url = `${API_URL}/roomAvailability/available-rooms?${queryParams.toString()}`;
    console.log(`Making request to: ${url}`);

    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), 20000); // 20s timeout

    const response = await fetch(url, {
      signal: controller.signal,
      mode: 'cors',
      headers: {
        'Accept': 'application/json',
      },
    });

    clearTimeout(timeoutId);

    if (!response.ok) {
      throw new Error(`Error ${response.status}: ${response.statusText}`);
    }

    return await response.json();
  } catch (error) {
    if (error.name === 'AbortError') {
      console.error('Request timed out');
      throw new Error('The request timed out. The server might be busy or unavailable.');
    }

    console.error('Error searching available rooms:', error);
    throw error;
  }
};
