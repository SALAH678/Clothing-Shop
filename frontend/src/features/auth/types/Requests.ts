export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  phoneNumber: string;
  email: string;
  password: string;
}

export interface ResendCodeRequest {
  email: string;
  verificationTokenType: "EmailVerification" | "PasswordReset";
}

export interface VerifyEmailRequest {
  email: string;
  code: string;
}
