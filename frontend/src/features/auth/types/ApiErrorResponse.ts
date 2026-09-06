export interface ApiErrorResponse {
  response?: {
    data?: {
      detail?: string;
      message?: string;
    };
  };
}