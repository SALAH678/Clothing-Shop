import { useMemo, useState, useEffect, useCallback, useRef, type ReactNode } from "react";
import { AuthContext } from "./authContext";
import type { LoginResponse, User } from "../types/LoginResponse";
import { setAccessToken as setApiAccessToken, setupAuthCallbacks } from "../../../lib/apiClient";
import * as authApi from "../api/authApi";
import { getRoleFromAccessToken, type AuthRole } from "../utils/jwt";

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
    } catch {
      // Session already expired server-side; still clear locally.
    } finally {
      setAccessTokenState(null);
      setUser(null);
      setApiAccessToken(null);
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
          setToken(null);
        }
      } catch {
        // Not logged in yet on fresh visit
        setToken(null);
        setUser(null);
      } finally {
        setIsLoading(false);
      }
    };

    initializeAuth();
  }, [login, setToken]);

  // Prefer the role from the user object; fall back to decoding the access
  // token's role claim so it still works if the user object is missing.
  const role = useMemo<AuthRole | null>(() => {
    if (user?.Role === "Admin" || user?.Role === "Customer") return user.Role;
    return getRoleFromAccessToken(accessToken);
  }, [user, accessToken]);

  const value = useMemo(
    () => ({
      accessToken,
      user,
      isAuthenticated: accessToken !== undefined && accessToken !== null,
      isLoading,
      role,
      isAdmin: role === "Admin",
      setToken,
      login,
      logout,
    }),
    [accessToken, user, isLoading, role, setToken, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
