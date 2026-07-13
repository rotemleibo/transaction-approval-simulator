import axios from 'axios';
import { tokenStorage } from '../../features/auth/tokenStorage';

const baseURL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';

export const unauthorizedEventName = 'transaction-approval:unauthorized';

export const http = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json',
  },
});

http.interceptors.request.use((config) => {
  const token = tokenStorage.getToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

http.interceptors.response.use(
  (response) => response,
  (error: unknown) => {
    if (axios.isAxiosError(error) && error.response?.status === 401) {
      tokenStorage.clear();
      window.dispatchEvent(new Event(unauthorizedEventName));
    }

    return Promise.reject(error);
  },
);
