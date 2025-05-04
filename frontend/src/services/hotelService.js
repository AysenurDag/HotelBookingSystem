const API_URL = process.env.REACT_APP_API_KEY;

export const searchHotels = async (searchParams) => {
  try {
    const queryParams = new URLSearchParams();
    
    if (searchParams.destination) {
      queryParams.set('city', searchParams.destination);
    }
    
    queryParams.set('page', searchParams.page || 1);
    queryParams.set('per_page', searchParams.perPage || 10);
    
    console.log(`Making request to: ${API_URL}/hotels?${queryParams.toString()}`);
    
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), 20000); // 20 second timeout
    
    const response = await fetch(`${API_URL}/hotels?${queryParams.toString()}`, {
      signal: controller.signal,
      mode: 'cors',
      headers: {
        'Accept': 'application/json'
      }
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