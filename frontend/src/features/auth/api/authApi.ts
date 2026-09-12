import { apiClient } from "../../../lib/apiClient";
import type { LoginResponse } from "../types/LoginResponse";
import type {
  GoogleRegisterRequest,
  LoginRequest,
  RegisterRequest,
  VerifyEmailRequest,
  ResendCodeRequest,
} from "../types/Requests";

export async function register(data: RegisterRequest): Promise<string> {
  const response = await apiClient.post("/auth/register", data);
  return response.data;
}

export async function verifyEmail(data: VerifyEmailRequest): Promise<void> {
  const response = await apiClient.post("/auth/verify-email", data);
  return response.data;
}

export async function login(data: LoginRequest): Promise<LoginResponse> {
  const response = await apiClient.post("/auth/login", data);
  return response.data;
}

export async function registerWithGoogle(data: GoogleRegisterRequest): Promise<LoginResponse> {
  const response = await apiClient.post("/auth/google", data);
  return response.data;
}

export async function logout(): Promise<void> {
  const response = await apiClient.post("/auth/logout");
  return response.data;
}

export async function refresh(): Promise<LoginResponse> {
  const response = await apiClient.post("/auth/refresh");
  return response.data;
}

export async function resendcode(data: ResendCodeRequest): Promise<string> {
  const response = await apiClient.post("/auth/resend-code", data);
  return response.data;
}

export async function forgotPassword(email: string): Promise<string> {
  const response = await apiClient.post("/auth/forgot-password", { email });
  return response.data;
}

export async function resetPassword(email: string, code: string, newPassword: string): Promise<string> {
  const response = await apiClient.post("/auth/reset-password", { email, code, newPassword });
  return response.data;
}
