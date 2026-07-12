import { useQuery } from '@tanstack/react-query';
import { getRegions } from '../../../services/api/regionsApi';

export function useRegions() {
  return useQuery({
    queryKey: ['regions'],
    queryFn: getRegions,
    staleTime: 5 * 60 * 1000,
  });
}
