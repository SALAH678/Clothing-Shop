import axios, { type InternalAxiosRequestConfig, AxiosError } from "axios";
import type { LoginResponse } from "../features/auth/types/LoginResponse";

// Normalize once so we never emit double slashes like `https://api//api`.
const rawApiUrl = (import.meta.env.VITE_API_URL ?? "").replace(/\/+$/, "");

export const apiClient = axios.create({
  baseURL: `${rawApiUrl}/api`,
  withCredentials: true, // required for refreshToken httpOnly cookie
  timeout: 15000, // never hang a request forever
});

// In-memory token management
let currentAccessToken: string | null | undefined = undefined;
let onTokenRefreshedCallback: ((data: LoginResponse) => void) | null = null;
let onLogoutCallback: (() => void) | null = null;

export const setAccessToken = (token: string | null | undefined) => {
  currentAccessToken = token;
};

export const getAccessToken = (): string | null | undefined => {
  return currentAccessToken;
};

export const setupAuthCallbacks = (
  onTokenRefreshed: (data: LoginResponse) => void,
  onLogout: () => void
) => {
  onTokenRefreshedCallback = onTokenRefreshed;
  onLogoutCallback = onLogout;
};

// Request Interceptor: Attach access token if present
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    if (currentAccessToken && !config.headers.Authorization) {
      config.headers.Authorization = `Bearer ${currentAccessToken}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response Interceptor: Handle 401 and queue refresh
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (value?: unknown) => void;
  reject: (reason?: unknown) => void;
}> = [];

const processQueue = (error: unknown, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & {
      _retry?: boolean;
    };

    // If there is no response or it is not a 401 error, reject immediately
    if (!error.response || error.response.status !== 401 || !originalRequest) {
      return Promise.reject(error);
    }

    // Do not intercept auth refresh or login failures to prevent infinite loops.
    // Match on the pathname only, so query strings cannot dodge the check.
    const requestPath = (originalRequest.url || "").split("?")[0];
    if (requestPath.endsWith("/auth/refresh") || requestPath.endsWith("/auth/login")) {
      return Promise.reject(error);
    }

    // If this request was already retried once, reject
    if (originalRequest._retry) {
      return Promise.reject(error);
    }

    // Mark every request that reaches here as retried — including ones
    // that get queued below (they didn't trigger the refresh, but a second
    // 401 on their replay must not start another refresh cycle).
    originalRequest._retry = true;

    // If refreshing is already in progress, push this request promise to queue
    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        failedQueue.push({ resolve, reject });
      })
        .then((token) => {
          if (token && typeof token === "string") {
            originalRequest.headers.Authorization = `Bearer ${token}`;
          }
          return apiClient(originalRequest);
        })
        .catch((err) => Promise.reject(err));
    }

    isRefreshing = true;

    try {
      // Refresh endpoint uses httpOnly cookie
      const { data } = await apiClient.post<LoginResponse>("/auth/refresh");
      const newAccessToken = data.accessToken;

      setAccessToken(newAccessToken);
      if (onTokenRefreshedCallback) {
        onTokenRefreshedCallback(data);
      }

      processQueue(null, newAccessToken);

      originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
      return apiClient(originalRequest);
    } catch (refreshError) {
      processQueue(refreshError, null);
      setAccessToken(null);
      if (onLogoutCallback) {
        onLogoutCallback();
      }
      return Promise.reject(refreshError);
    } finally {
      isRefreshing = false;
    }
  }
);