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

export const getBookingsByUserId = async (userId) => {
  try {
    const response = await fetch(`${API_URL}/bookings/user/${userId}`, {
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

// bookingService.js
export const cancelBooking = async (bookingId) => {
  try {
    const reason = encodeURIComponent("Cancelled by user");
    const response = await fetch(`${API_URL}/bookings/${bookingId}/cancel?reason=${reason}`, {
      method: 'POST',
      headers: {
        Accept: '*/*',
      },
      body: "Kullanıcı iptali", // çünkü -d '' kullanılmış
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(`Cancel failed: ${response.status} ${error}`);
    }

    return await response.json();
  } catch (error) {
    console.error('❌ Cancel Booking Error:', error);
    throw error;
  }
};
