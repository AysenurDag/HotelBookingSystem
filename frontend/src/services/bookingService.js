// src/services/bookingService.js
import { msalInstance } from '../msalInstance';
import { loginRequest } from '../msalConfig';
const API_URL = process.env.REACT_APP_BOOKING_API;


export const createBooking = async (bookingData) => {
  try {
    const accounts = msalInstance.getAllAccounts();
    const result = await msalInstance.acquireTokenSilent({
      ...loginRequest,
      account: accounts[0],
    });

    const token = result.accessToken;

    const response = await fetch(`${API_URL}/bookings`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Accept: '*/*',
        Authorization: `Bearer ${token}` // ✅ EKLENDİ
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
