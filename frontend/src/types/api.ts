export type Region = {
  code: string;
  name: string;
  timeZoneId: string;
};

export type TransactionStatus = 'Approved' | 'Rejected';

export type SimulateTransactionRequest = {
  regionCode: string;
  submittedAt: string;
};

export type SimulateTransactionResponse = {
  id: string;
  regionCode: string;
  regionName: string;
  timeZoneId: string;
  submittedUtc: string;
  localTransactionTime: string;
  status: TransactionStatus;
  reason: string;
};

export type ApprovedTransaction = {
  id: string;
  regionCode: string;
  regionName: string;
  timeZoneId: string;
  submittedUtc: string;
  localTransactionTime: string;
  createdAtUtc: string;
};

export type PagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type AuthRequest = {
  username: string;
  password: string;
};

export type AuthResponse = {
  username: string;
  token: string;
  expiresAtUtc: string;
};
