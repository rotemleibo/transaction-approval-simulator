import { useInfiniteQuery } from '@tanstack/react-query';
import { getApprovedTransactions } from '../../../services/api/transactionsApi';

export function useApprovedTransactions(pageSize = 20, enabled = true) {
  return useInfiniteQuery({
    queryKey: ['approved-transactions', pageSize],
    queryFn: ({ pageParam }) => getApprovedTransactions(pageParam, pageSize),
    initialPageParam: 1,
    getNextPageParam: (lastPage) =>
      lastPage.page < lastPage.totalPages ? lastPage.page + 1 : undefined,
    enabled,
  });
}
