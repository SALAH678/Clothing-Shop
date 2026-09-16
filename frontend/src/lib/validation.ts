/**
 * Single source of truth for auth-form validation.
 *
 * These rules were previously copy-pasted into Login, Register and
 * ForgotPassword, which meant a fix in one form silently left the others
 * inconsistent. Import these instead of re-declaring regexes inline.
 */

// Name: letters and spaces only.
export const NAME_PATTERN = /^[A-Za-z\s]+$/;

// Algerian mobile: starts 05/06/07 followed by 8 digits.
export const ALGERIAN_PHONE_PATTERN = /^0[5-7][0-9]{8}$/;

// The product deliberately restricts accounts to Gmail addresses.
// NOTE: this is stricter than the Google OAuth flow, which also accepts
// Google Workspace domains - see the auth notes in the project docs.
export const GMAIL_PATTERN = /^[a-zA-Z0-9._%+-]+@gmail\.com$/;

// Min 6 chars with at least one lower, upper, digit and special character.
export const STRONG_PASSWORD_PATTERN = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{6,}$/;

export const MIN_PASSWORD_LENGTH = 6;

/**
 * These factories return *inferred literal* types rather than an explicit
 * `RegisterOptions` annotation.
 *
 * Annotating with `RegisterOptions` widens the type (notably `deps` becomes
 * `string | string[]`) which is then not assignable to the field-name union
 * that `useForm<SomeFormValues>()` produces, and `Omit<...>` cannot help
 * because `RegisterOptions` is a discriminated union of `pattern` /
 * `valueAsNumber` / `valueAsDate` variants. Letting TS infer keeps the exact
 * shape, which is structurally assignable to every typed form.
 */
export const nameRules = (label: string) => ({
  required: `${label} is required`,
  pattern: { value: NAME_PATTERN, message: "Must contain only characters and spaces" },
});

export const phoneRules = () => ({
  required: "Phone number is required",
  pattern: { value: ALGERIAN_PHONE_PATTERN, message: "Phone number must be a valid Algerian mobile number." },
});

export const gmailRules = () => ({
  required: "Email is required",
  pattern: { value: GMAIL_PATTERN, message: "Must be a valid @gmail.com address" },
});

export const passwordRules = () => ({
  required: "Password is required",
  minLength: { value: MIN_PASSWORD_LENGTH, message: `Must be at least ${MIN_PASSWORD_LENGTH} characters long` },
  pattern: {
    value: STRONG_PASSWORD_PATTERN,
    message: "Must contain at least 1 uppercase, 1 lowercase, 1 number, and 1 special character",
  },
});

/** Digits-only filter used by phone inputs to keep `onInput` handlers consistent. */
export const digitsOnly = (value: string): string => value.replace(/\D/g, "");