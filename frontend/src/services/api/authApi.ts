import { http } from './http';
import type { AuthRequest, AuthResponse } from '../../types/api';

export async function signup(request: AuthRequest): Promise<AuthResponse> {
  const response = await http.post<AuthResponse>('/api/auth/signup', request);
  return response.data;
}

export async function login(request: AuthRequest): Promise<AuthResponse> {
  const response = await http.post<AuthResponse>('/api/auth/login', request);
  return response.data;
}
