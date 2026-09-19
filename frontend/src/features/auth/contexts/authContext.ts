import { createContext } from "react";
import type { LoginResponse, User } from "../types/LoginResponse";
import type { AuthRole } from "../utils/jwt";

export interface AuthContextType {
  accessToken: string | null | undefined;
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  /** Resolved from the user object, falling back to the access token's role claim. */
  role: AuthRole | null;
  /** True only when the authenticated user has the Admin role. */
  isAdmin: boolean;
  setToken: (token: string | null | undefined) => void;
  login: (data: LoginResponse) => void;
  logout: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);
