// src/services/hotelService.js
const API_URL = process.env.REACT_APP_HOTEL_SERVICE_API_URL;

export const searchHotels = async (searchParams) => {
    try {
      const queryParams = new URLSearchParams();
      
      if (searchParams.destination) {
        queryParams.set('city', searchParams.destination);
      }
      
      // Add pagination
      queryParams.set('page', searchParams.page || 1);
      queryParams.set('per_page', searchParams.perPage || 10);
      
      // Other search parameters can be added here
      
      const response = await fetch(`${API_URL}/hotels?${queryParams.toString()}`);
      console.log(API_URL)

      if (!response.ok) {
        throw new Error(`Error ${response.status}: ${response.statusText}`);
      }
      
      return await response.json();
    } catch (error) {
      console.error('Error searching hotels:', error);
      throw error;
    }
  };
  
  export const getHotelById = async (hotelId) => {
    try {
      const response = await fetch(`${API_URL}/hotels/${hotelId}`);
      
      if (!response.ok) {
        throw new Error(`Error ${response.status}: ${response.statusText}`);
      }
      
      return await response.json();
    } catch (error) {
      console.error(`Error fetching hotel with ID ${hotelId}:`, error);
      throw error;
    }
  };