import { createContext } from "react";
import type { LoginResponse, User } from "../types/LoginResponse";

export interface AuthContextType {
  accessToken: string | null | undefined;
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  setToken: (token: string | null | undefined) => void;
  login: (data: LoginResponse) => void;
  logout: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);
