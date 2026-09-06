import { useMutation, useQueryClient } from "@tanstack/react-query";
import * as authApi from "../api/authApi";
import { useAuth } from "./useAuth";
import type {
  LoginRequest,
  RegisterRequest,
  VerifyEmailRequest,
  ResendCodeRequest,
} from "../types/Requests";

export function useLogin() {
  const { login } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: LoginRequest) => authApi.login(data),
    onSuccess: (data) => {
      login(data);
      queryClient.clear();
    },
  });
}

export function useRegister() {
  return useMutation({
    mutationFn: (data: RegisterRequest) => authApi.register(data),
  });
}

export function useVerifyEmail() {
  return useMutation({
    mutationFn: (data: VerifyEmailRequest) => authApi.verifyEmail(data),
  });
}

export function useResendCode() {
  return useMutation({
    mutationFn: (data: ResendCodeRequest) => authApi.resendcode(data),
  });
}

export function useForgotPassword() {
  return useMutation({
    mutationFn: (email: string) => authApi.forgotPassword(email),
  });
}

export function useResetPassword() {
  return useMutation({
    mutationFn: ({
      email,
      code,
      newPassword,
    }: {
      email: string;
      code: string;
      newPassword: string;
    }) => authApi.resetPassword(email, code, newPassword),
  });
}

export function useLogout() {
  const { logout } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => logout(),
    onSuccess: () => {
      queryClient.clear();
    },
  });
}
