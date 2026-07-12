import { useMutation, useQueryClient } from '@tanstack/react-query';
import { simulateTransaction } from '../../../services/api/transactionsApi';

export function useSimulateTransaction() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: simulateTransaction,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['approved-transactions'] });
    },
  });
}
