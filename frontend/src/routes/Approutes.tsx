import { Route, Routes } from "react-router-dom";
import Home from "../pages/Home";
import MainLayout from "../layouts/Mainlayout";
import CategoriesOverview from "../pages/CategoriesOverview";
import CategoryProducts from "../pages/CategoryProducts";

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
        </Route>
      </Routes>
    </>
  );
}
