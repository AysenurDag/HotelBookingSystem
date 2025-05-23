const API_URL = process.env.REACT_APP_BOOKING_API || 'http://localhost:8081/api'; // .env üzerinden de ayarlanabilir

export const createBooking = async (bookingData) => {
  try {
    const response = await fetch(`${API_URL}/bookings`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Accept: '*/*',
      },
      body: JSON.stringify(bookingData),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(`Booking failed: ${response.status} ${error}`);
    }

    return await response.json();
  } catch (error) {
    console.error('❌ Booking Error:', error);
    throw error;
  }
};

// Tüm rezervasyonları getirir
export const getAllBookings = async () => {
  try {
    const response = await fetch(`${API_URL}/bookings`, {
      method: 'GET',
      headers: {
        Accept: '*/*',
      },
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(`Fetching bookings failed: ${response.status} ${error}`);
    }

    return await response.json();
  } catch (error) {
    console.error('❌ Get Bookings Error:', error);
    throw error;
  }
};