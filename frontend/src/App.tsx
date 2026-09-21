import { BrowserRouter } from "react-router-dom";
import { GoogleOAuthProvider } from "@react-oauth/google";
import AppRoutes from "./routes/AppRoutes";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AuthProvider } from "./features/auth/contexts/authProvider";
import { CartProvider } from "./features/carts/contexts/CartContext";
import { isRateLimitError } from "./lib/rateLimit";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 60 * 1000,
      gcTime: 5 * 60 * 1000,
      // Never auto-retry a 429 — the backend is already telling us to back off;
      // an instant retry would just hammer the limiter again.
      retry: (failureCount, error) => (isRateLimitError(error) ? false : failureCount < 1),
      refetchOnWindowFocus: false,
    },
  },
});

const googleClientId = import.meta.env.VITE_GOOGLE_CLIENT_ID as string | undefined;

export default function App() {
  if (!googleClientId) {
    console.warn("[app] VITE_GOOGLE_CLIENT_ID is missing; Google login will be disabled.");
  }
  return (
    <QueryClientProvider client={queryClient}>
      <GoogleOAuthProvider clientId={googleClientId ?? "missing-client-id"}>
        <BrowserRouter>
          <AuthProvider>
            <CartProvider>
              <AppRoutes />
            </CartProvider>
          </AuthProvider>
        </BrowserRouter>
      </GoogleOAuthProvider>
    </QueryClientProvider>
  );
}
