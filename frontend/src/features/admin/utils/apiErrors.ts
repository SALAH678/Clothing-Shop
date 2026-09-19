import type { ApiErrorResponse } from "../../auth/types/ApiErrorResponse";

/** Titles ASP.NET Core uses for generic validation problems (not worth showing). */
const GENERIC_VALIDATION_TITLE = "One or more validation errors occurred.";

/** Extracts the most useful message out of a ProblemDetails / ValidationProblemDetails response. */
export function getApiErrorMessage(error: unknown, fallback: string): string {
  const data = (error as ApiErrorResponse | undefined)?.response?.data;

  const validationMessage = data?.errors
    ? Object.values(data.errors)
        .flat()
        .find((message) => Boolean(message))
    : undefined;

  if (validationMessage) return validationMessage;
  if (data?.title && data.title !== GENERIC_VALIDATION_TITLE) return data.title;
  return data?.message ?? data?.detail ?? fallback;
}