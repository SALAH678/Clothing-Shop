import { Outlet } from "react-router-dom";

import Navbar from "../components/layout/Navbar";
import Footer from "../components/layout/Footer";
import ScrollToTop from "../components/ui/ScrollToTop";
import CartDrawer from "../features/carts/components/CartDrawer";

export default function MainLayout() {
  return (
    <>
      <ScrollToTop />
      <Navbar />
      <CartDrawer />

      <main>
        <Outlet />
      </main>

      <Footer />
    </>
  );
}
