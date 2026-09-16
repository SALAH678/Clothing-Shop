import { Suspense } from "react";
import { Outlet } from "react-router-dom";

import Navbar from "../components/layout/Navbar";
import Footer from "../components/layout/Footer";
import ScrollToTop from "../components/ui/ScrollToTop";
import CartDrawer from "../features/carts/components/CartDrawer";
import PageLoader from "../components/ui/PageLoader";

export default function MainLayout() {
  return (
    <>
      <ScrollToTop />
      <Navbar />
      <CartDrawer />

      <main>
        {/* Suspense lives below the chrome so lazy page chunks never unmount the navbar. */}
        <Suspense fallback={<PageLoader />}>
          <Outlet />
        </Suspense>
      </main>

      <Footer />
    </>
  );
}
