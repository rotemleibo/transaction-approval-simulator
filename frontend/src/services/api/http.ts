import axios from 'axios';
import { tokenStorage } from '../../features/auth/tokenStorage';

const baseURL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';

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
