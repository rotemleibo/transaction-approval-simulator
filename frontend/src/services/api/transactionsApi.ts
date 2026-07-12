import { http } from './http';
import type {
  ApprovedTransaction,
  PagedResult,
  SimulateTransactionRequest,
  SimulateTransactionResponse,
} from '../../types/api';

export async function simulateTransaction(
  request: SimulateTransactionRequest,
): Promise<SimulateTransactionResponse> {
  const response = await http.post<SimulateTransactionResponse>('/api/transactions/simulate', request);
  return response.data;
}

export async function getApprovedTransactions(
  page = 1,
  pageSize = 20,
): Promise<PagedResult<ApprovedTransaction>> {
  const response = await http.get<PagedResult<ApprovedTransaction>>('/api/transactions/approved', {
    params: { page, pageSize },
  });
  return response.data;
}
