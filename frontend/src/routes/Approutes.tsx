import { Route, Routes } from "react-router-dom";
import Home from "../pages/Home";
import MainLayout from "../layouts/Mainlayout";
import CategoriesOverview from "../pages/CategoriesOverview";
import CategoryProducts from "../pages/CategoryProducts";
import Register from "../pages/Register";
import Login from "../pages/Login";
import ForgotPassword from "../pages/ForgotPassword";
import ProtectedRoute from "../features/auth/components/ProtoctedRoute";
import Profile from "../pages/Profile";
import VerifyEmail from "../pages/VerifyEmail";

export default function Approutes() {
  return (
    <>
      <Routes>
        <Route element={<MainLayout />}>
          <Route path="/" element={<Home />} />

          <Route path="/categories">
            <Route index element={<CategoriesOverview />} />
            <Route path=":categoryName" element={<CategoryProducts />} />
          </Route>

          <Route path="/auth">
            <Route path="login" element={<Login />} />
            <Route path="register" element={<Register />} />
            <Route path="verify-email" element={<VerifyEmail />} />
            <Route path="forgot-password" element={<ForgotPassword />} />
          </Route>
        </Route>

        <Route element={<ProtectedRoute />}>
          <Route element={<MainLayout />}>
            <Route path="/profile" element={<Profile />} />
          </Route>
        </Route>
      </Routes>
    </>
  );
}
