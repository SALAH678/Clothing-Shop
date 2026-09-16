import { BrowserRouter } from "react-router-dom";
import { GoogleOAuthProvider } from "@react-oauth/google";
import AppRoutes from "./routes/AppRoutes";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AuthProvider } from "./features/auth/contexts/authProvider";
import { CartProvider } from "./features/carts/contexts/CartContext";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 60 * 1000,
      gcTime: 5 * 60 * 1000,
      retry: 1,
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
