import { useQuery } from '@tanstack/react-query';
import { getApprovedTransactions } from '../../../services/api/transactionsApi';

export function useApprovedTransactions(page = 1, pageSize = 20, enabled = true) {
  return useQuery({
    queryKey: ['approved-transactions', page, pageSize],
    queryFn: () => getApprovedTransactions(page, pageSize),
    enabled,
  });
}
