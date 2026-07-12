import { http } from './http';
import type { Region } from '../../types/api';

export async function getRegions(): Promise<Region[]> {
  const response = await http.get<Region[]>('/api/regions');
  return response.data;
}
