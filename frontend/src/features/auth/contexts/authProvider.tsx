import { useState, useEffect, useCallback, useRef, type ReactNode } from "react";
import { AuthContext } from "./authContext";
import type { LoginResponse, User } from "../types/LoginResponse";
import { setAccessToken as setApiAccessToken, setupAuthCallbacks } from "../../../lib/apiClient";
import * as authApi from "../api/authApi";

export function AuthProvider({ children }: { children: ReactNode }) {
  const [accessToken, setAccessTokenState] = useState<string | null | undefined>(undefined);
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const authInitializationStarted = useRef(false);

  const setToken = useCallback((token: string | null | undefined) => {
    setAccessTokenState(token);
    setApiAccessToken(token);
  }, []);

  const login = useCallback((data: LoginResponse) => {
    setAccessTokenState(data.accessToken);
    setUser(data.user);
    setApiAccessToken(data.accessToken);
  }, []);

  const logout = useCallback(async () => {
    try {
      await authApi.logout();
    } catch (err) {
      console.error("Logout error:", err);
    } finally {
      setAccessTokenState(undefined);
      setUser(null);
      setApiAccessToken(undefined);
    }
  }, []);

  // Sync callbacks with axios interceptor
  useEffect(() => {
    setupAuthCallbacks(
      (data: LoginResponse) => {
        setAccessTokenState(data.accessToken);
        setUser(data.user);
      },
      () => {
        // When interceptor fails refresh or token is expired, set to null
        setAccessTokenState(null);
        setUser(null);
      },
    );
  }, []);

  // Silent refresh on initial app load to restore session if refreshToken cookie exists
  useEffect(() => {
    if (authInitializationStarted.current) return;
    authInitializationStarted.current = true;

    const initializeAuth = async () => {
      try {
        const data = await authApi.refresh();
        if (data?.accessToken) {
          login(data);
        } else {
          setToken(undefined);
        }
      } catch {
        // Not logged in yet on fresh visit
        setToken(undefined);
        setUser(null);
      } finally {
        setIsLoading(false);
      }
    };

    initializeAuth();
  }, [login, setToken]);

  const isAuthenticated = !!accessToken;

  return (
    <AuthContext.Provider
      value={{
        accessToken,
        user,
        isAuthenticated,
        isLoading,
        setToken,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}
