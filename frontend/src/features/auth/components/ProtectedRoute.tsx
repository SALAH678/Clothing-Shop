import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "../hooks/useAuth";
import type { ReactNode } from "react";

interface ProtectedRouteProps {
  children?: ReactNode;
}

export default function ProtectedRoute({ children }: ProtectedRouteProps) {
  const { accessToken, isLoading } = useAuth();
  const location = useLocation();

  // While checking initial refresh / restoring token session on load
  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[50vh]">
        <div className="font-mono text-sm font-bold uppercase animate-pulse">Loading session...</div>
      </div>
    );
  }

  // If user is not logged in or token is expired, redirect to login page
  // We pass state={{ from: location }} so we can redirect back after successful login
  if (accessToken === null || accessToken === undefined) {
    return <Navigate to="/auth/login" state={{ from: location }} replace />;
  }

  // If children are passed, render them; otherwise render the nested Outlet
  return children ? <>{children}</> : <Outlet />;
}
