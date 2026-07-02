import { ApiResponse } from "@/api/apiTypes";
export const getErrorMessages = (
  error: ApiResponse<null> | null | undefined
): string[] => {
  if (!error) return ['An unexpected error occurred.'];

  // Case 1: error.error is an array of strings
  if (Array.isArray(error.error) && error.error.length > 0) {
    return error.error;
  }

  // Case 2: error.error is a string
  if (typeof error.error === 'string' && error.error.trim()) {
    return [error.error];
  }

  // ✅ Case 3: error.error is an object with { error: string }
  const nestedError: unknown = error.error;
  if (
    nestedError != null &&
    typeof nestedError === 'object' &&
    'error' in nestedError &&
    typeof (nestedError as { error?: string }).error === 'string'
  ) {
    return [(nestedError as { error: string }).error];
  }

  // Case 4: fallback to top-level message
  if (typeof error.message === 'string' && error.message.trim()) {
    return [error.message];
  }

  return ['An unknown error occurred.'];
};
