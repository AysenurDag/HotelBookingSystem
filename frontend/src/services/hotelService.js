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
