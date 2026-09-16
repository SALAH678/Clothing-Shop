import { Suspense, lazy } from "react";
import { Route, Routes } from "react-router-dom";
import Home from "../pages/Home";
import MainLayout from "../layouts/MainLayout";
import ProtectedRoute from "../features/auth/components/ProtectedRoute";
import EmptyState from "../components/ui/EmptyState";
import PageLoader from "../components/ui/PageLoader";

// Route-level code splitting: `Home` stays eager for the fastest first paint,
// every other page is fetched on demand.
const CategoriesOverview = lazy(() => import("../pages/CategoriesOverview"));
const CategoryProducts = lazy(() => import("../pages/CategoryProducts"));
const ProductDetails = lazy(() => import("../pages/ProductDetails"));
const Register = lazy(() => import("../pages/Register"));
const Login = lazy(() => import("../pages/Login"));
const ForgotPassword = lazy(() => import("../pages/ForgotPassword"));
const VerifyEmail = lazy(() => import("../pages/VerifyEmail"));
const Profile = lazy(() => import("../pages/Profile"));
const Checkout = lazy(() => import("../pages/CheckOut"));
const CheckoutSuccess = lazy(() => import("../pages/CheckOutSuccess"));
const CheckoutFailed = lazy(() => import("../pages/CheckOutFailed"));

export default function AppRoutes() {
  return (
    <Routes>
      <Route element={<MainLayout />}>
        <Route path="/" element={<Home />} />

        <Route path="/categories">
          <Route index element={<CategoriesOverview />} />
          <Route path=":categoryName" element={<CategoryProducts />} />
          <Route path=":categoryName/:id" element={<ProductDetails />} />
        </Route>

        <Route path="/auth">
          <Route path="login" element={<Login />} />
          <Route path="register" element={<Register />} />
          <Route path="verify-email" element={<VerifyEmail />} />
          <Route path="forgot-password" element={<ForgotPassword />} />
        </Route>
      </Route>

      <Route element={<ProtectedRoute />}>
        {/* These routes sit outside MainLayout, so they own their fallback. */}
        <Route
          path="/checkout"
          element={
            <Suspense fallback={<PageLoader label="Loading checkout" />}>
              <Checkout />
            </Suspense>
          }
        />
        <Route
          path="/checkout/success"
          element={
            <Suspense fallback={<PageLoader label="Loading" />}>
              <CheckoutSuccess />
            </Suspense>
          }
        />
        <Route
          path="/checkout/failed"
          element={
            <Suspense fallback={<PageLoader label="Loading" />}>
              <CheckoutFailed />
            </Suspense>
          }
        />
        <Route element={<MainLayout />}>
          <Route path="/profile" element={<Profile />} />
        </Route>
      </Route>

      <Route
        path="*"
        element={<EmptyState title="Page not found" message="The page you are looking for does not exist." />}
      />
    </Routes>
  );
}