import { getConfig } from '../config/config'


import { getAuthToken } from '../helpers/auth-token-helper'
import { handleApiResponse } from '../helpers/auth-token-helper'

export async function loginUser({ Username, PasswordString }) {
  try {
    const config = await getConfig(); // load config at runtime
    const API_BASE_URL = config.API_BASE_URL;
    const API_AUTH_TOKEN = config.API_AUTH_TOKEN;

    const response = await fetch(`${API_BASE_URL}/api/user/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Api-Token': API_AUTH_TOKEN
        
      },
      body: JSON.stringify({ Username, PasswordString }),
    });

    const data = await response.json();
    return data;
  } catch (error) {
    console.error("Login API error:", error);
    throw error;
  }
}


export async function logoutUser() {
  const config = await getConfig(); // load config at runtime
  const API_BASE_URL = config.API_BASE_URL;
  const API_AUTH_TOKEN = config.API_AUTH_TOKEN;

  const token = getAuthToken();

  if (!token) return false; 

  try {

    
    const response = await fetch(`${API_BASE_URL}/api/auth/logout`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
        'X-Api-Token': API_AUTH_TOKEN
      }
    });

    const data = await response.json();
    console.log('Logout response:', data);

    return data; 
  } catch (error) {
    console.error('Logout API error:', error);
    // still clear local storage even if API fails
    return false;
  }
}

export async function createUser(userData) {
  const config = await getConfig(); // load config at runtime
  const API_BASE_URL = config.API_BASE_URL;
  const API_AUTH_TOKEN = config.API_AUTH_TOKEN;

  const token = getAuthToken();

  try {
    const response = await fetch(`${API_BASE_URL}/api/user/create`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Api-Token': API_AUTH_TOKEN,
        'Authorization': `Bearer ${token}`, // token needed for auth check
      },
      body: JSON.stringify(userData),
    });

    return handleApiResponse(response);

  } catch (error) {
    console.error('CreateUser API error:', error);
    throw error;
  }
}




export async function getUsers(searchTerm = null) {
  const config = await getConfig(); // load config at runtime
  const API_AUTH_TOKEN = config.API_AUTH_TOKEN;

  const token = getAuthToken();

  const baseUrl = `${config.API_BASE_URL}/api/user/get-user/`;

  const params = new URLSearchParams();
    if (searchTerm) params.append('searchTerm', searchTerm);
    
  const queryString = params.toString();
  const finalUrl = queryString ? `${baseUrl}?${queryString}` : baseUrl;

  try {
    const response = await fetch(finalUrl, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'X-Api-Token': API_AUTH_TOKEN,
        'Authorization': `Bearer ${token}`,
      },
    });

    return handleApiResponse(response);

  } catch (error) {
    console.error('GetUsers API error:', error);
    throw error;
  }
}



export async function updateUserDetails(userData) {
  const config = await getConfig(); // load config at runtime
  const API_BASE_URL = config.API_BASE_URL;
  const API_AUTH_TOKEN = config.API_AUTH_TOKEN;

  const token = getAuthToken();

  try {
    const response = await fetch(`${API_BASE_URL}/api/user/update`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Api-Token': API_AUTH_TOKEN,
        'Authorization': `Bearer ${token}`,
      },
      body: JSON.stringify(userData),
    });

    return handleApiResponse(response);

  } catch (error) {
    console.error('UpdateUser API error:', error);
    throw error;
  }
}

export async function updateUserStatus(id, isDeleted, username) {
  const config = await getConfig(); // load config at runtime
  const API_BASE_URL = config.API_BASE_URL;
  const API_AUTH_TOKEN = config.API_AUTH_TOKEN;
  
  const token = getAuthToken();

  try {
    const payload = {
      Id: id,
      Username: username,
      IsDeleted: isDeleted,
      UpdatedDate: new Date().toISOString(),
      UpdatedBy: 1,
    };

    const response = await fetch(`${API_BASE_URL}/api/user/update`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Api-Token': API_AUTH_TOKEN,
        'Authorization': `Bearer ${token}`,
      },
      body: JSON.stringify(payload),
    });

    // Attempt to parse the body even on errors
    const responseBody = await response.json().catch(() => ({}));

    // If the server responded with 401 or another error
    if (!response.ok) {
      const serverMessage = responseBody?.Message || 'Unauthorized or invalid token.';
      // Optionally, if 401, handle logout or redirect here
      if (response.status === 401) {
        console.warn('Token expired or unauthorized — logging out...');
        localStorage.clear();
        window.location.href = '/'; // redirect to login
      }
      throw new Error(serverMessage);
    }

    return responseBody;
  } catch (error) {
    console.error('UpdateUserStatus API error:', error);
    throw error;
  }
}


export async function changeUserPassword(id, newPassword) {
  try {
    const config = await getConfig(); // load config at runtime
    const API_BASE_URL = config.API_BASE_URL;
    const API_AUTH_TOKEN = config.API_AUTH_TOKEN;

    const token = getAuthToken();
    const response = await fetch(`${API_BASE_URL}/api/user/change-password`, {
      method: 'POST',
      headers: new Headers({
        'Authorization': `Bearer ${token}`,
        'X-Api-Token': API_AUTH_TOKEN,
        'Content-Type': 'application/json',
      }),
      body: JSON.stringify({ Id: id, PasswordString: newPassword }),
      credentials: 'include',
    });

    return handleApiResponse(response);
  } catch (error) {
    console.error('Change password API error:', error);
    throw error;
  }
}