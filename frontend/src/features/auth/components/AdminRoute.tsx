import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../hooks/useAuth";

/**
 * Role guard for admin-only routes. MUST be nested inside `ProtectedRoute`,
 * which guarantees an authenticated session — this component only checks that
 * the authenticated user's role is Admin and redirects everyone else home.
 */
export default function AdminRoute() {
  const { isAdmin, isLoading } = useAuth();

  // Same silent-wait pattern as ProtectedRoute so an admin whose session is
  // still being restored isn't bounced home before the token arrives.
  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[50vh]">
        <div className="font-mono text-sm font-bold uppercase animate-pulse">Loading session...</div>
      </div>
    );
  }

  if (!isAdmin) {
    return <Navigate to="/" replace />;
  }

  return <Outlet />;
}