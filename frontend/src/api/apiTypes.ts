export interface ApiResponse<T = unknown> {
  message: string;
  status: boolean;
  data?: T;
  error?: string;
  statusCode?: number;
}

