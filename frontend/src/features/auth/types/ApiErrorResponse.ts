export interface ApiErrorResponse {
  response?: {
    data?: {
      detail?: string;
      message?: string;
      /** Problem-details title (RFC 7807). */
      title?: string;
      /** FluentValidation failures keyed by property name. */
      errors?: Record<string, string[]>;
    };
  };
}