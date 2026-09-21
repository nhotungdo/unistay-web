import axios from 'axios';

function hardResetToLogin() {
  localStorage.removeItem('token');
  localStorage.removeItem('user');
  // Full reload intentionally clears all in-memory app state (SignalR
  // connections, caches, auth store) before restarting the session.
  window.location.assign('/login');
}

// Create an Axios instance
const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5246/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add a request interceptor to attach the JWT token
api.interceptors.request.use(
  (config) => {
    // We only access localStorage in the browser
    if (typeof window !== 'undefined') {
      const token = localStorage.getItem('token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Add a response interceptor to handle errors (e.g., unauthorized)
api.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    if (error.response && error.response.status === 401) {
      if (typeof window !== 'undefined') {
        hardResetToLogin();
      }
    }
    return Promise.reject(error);
  }
);

export default api;
