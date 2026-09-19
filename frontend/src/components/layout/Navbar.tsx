import { useState } from "react";
import { Search, ShoppingCart, User, Settings } from "lucide-react";
import { Link, useNavigate, useLocation, useSearchParams } from "react-router-dom";
import { useAuth } from "../../features/auth/hooks/useAuth";
import { useCart } from "../../features/carts/hooks/useCart";
import { useCategories } from "../../features/categories/hooks/useCategories";
import { createSlug } from "../ui/Slug";
import type { Category } from "../../features/categories/types/Category";

const NAVBAR_CATEGORY_NAMES = ["T-Shirt Oversize", "Pants", "Old Money Shirts", "Sneakers", "Ultra Baggy"];

export default function Navbar() {
  const navigate = useNavigate();
  const location = useLocation();
  const [searchParams] = useSearchParams();
  const { isAuthenticated, isAdmin } = useAuth();
  const { cartCount, setIsCartOpen } = useCart();
  const { data: categories } = useCategories();

  // Derive the nav from real categories so links never point at a 404 slug.
  const navCategories: Category[] = NAVBAR_CATEGORY_NAMES.map((name) =>
    categories?.find((category: Category) => category.categoryName === name),
  ).filter((category: Category | undefined): category is Category => Boolean(category));

  const currentSearchParam = searchParams.get("search") ?? "";
  // Controlled inputs keep focus across navigations; search only runs when the
  // user submits (search-icon click or Enter), never while typing.
  const [mobileSearch, setMobileSearch] = useState(currentSearchParam);
  const [desktopSearch, setDesktopSearch] = useState(currentSearchParam);
  const [prevSearchParam, setPrevSearchParam] = useState(currentSearchParam);

  // Keep both inputs in sync when the URL changes from outside the input
  // (back/forward, header links, "clear search"). Adjusted during render rather
  // than in an effect so the DOM is never painted with the stale value.
  if (prevSearchParam !== currentSearchParam) {
    setPrevSearchParam(currentSearchParam);
    setMobileSearch(currentSearchParam);
    setDesktopSearch(currentSearchParam);
  }

  const submitSearch = (raw: string) => {
    const trimmed = raw.trim();

    // Blur active input so it collapses back to icon
    if (document.activeElement instanceof HTMLElement) {
      document.activeElement.blur();
    }

    const isCategoryPage = location.pathname.startsWith("/categories/") && location.pathname !== "/categories";

    const targetPath = isCategoryPage ? location.pathname : "/categories/all";
    const nextParams = new URLSearchParams(isCategoryPage ? location.search : "");

    if (trimmed) {
      nextParams.set("search", trimmed);
    } else {
      nextParams.delete("search");
      if (targetPath === "/categories/all") {
        navigate("/categories");
        return;
      }
    }

    const queryString = nextParams.toString();
    navigate(`${targetPath}${queryString ? `?${queryString}` : ""}`);
  };

  const handleSearchSubmit = (e: React.FormEvent, scope: "mobile" | "desktop") => {
    e.preventDefault();
    const raw = scope === "mobile" ? mobileSearch : desktopSearch;
    submitSearch(raw);
  };

  const handleCartClick = () => {
    if (!isAuthenticated) {
      navigate("/auth/login", { state: { from: location } });
      return;
    }

    setIsCartOpen(true);
  };

  return (
    <header className="bg-surface sticky top-0 z-50 border-b border-primary flex flex-col md:flex-row justify-between items-center w-full px-4 md:px-4 lg:px-6 py-4 transition-all duration-300 gap-4 md:gap-6 lg:gap-8">
      <div className="flex items-center justify-between w-full md:w-auto shrink-0">
        <Link
          to="/"
          className="font-display text-xl md:text-base lg:text-2xl font-bold uppercase tracking-tighter text-primary whitespace-nowrap pr-2"
        >
          Clothing Shop
        </Link>
        <div className="flex gap-4 md:hidden items-center">
          <form
            onSubmit={(e) => handleSearchSubmit(e, "mobile")}
            className="group relative flex items-center justify-end h-6"
          >
            <button type="submit" aria-label="Search" className="z-10 focus:outline-none cursor-pointer">
              <Search className="w-6 h-6" />
            </button>
            <input
              type="text"
              name="search"
              data-search-scope="mobile"
              placeholder="Search..."
              value={mobileSearch}
              onChange={(e) => setMobileSearch(e.target.value)}
              className="absolute right-0 w-0 opacity-0 group-hover:w-32 group-hover:opacity-100 group-hover:border-primary focus:w-32 focus:opacity-100 focus:border-primary transition-all duration-300 ease-out bg-surface border-b-2 border-transparent font-mono text-xs py-1 pr-8 outline-none z-0 cursor-text"
            />
          </form>
          {isAdmin && (
            <Link to="/admin" aria-label="Admin settings">
              <Settings className="w-6 h-6 hover:scale-110 active:scale-95 transition-transform cursor-pointer" />
            </Link>
          )}
          <Link to="/profile">
            <User className="w-6 h-6 hover:scale-110 active:scale-95 transition-transform cursor-pointer" />
          </Link>
          <button type="button" onClick={handleCartClick} className="relative" aria-label="Open cart">
            <ShoppingCart className="w-6 h-6 hover:opacity-70 transition-opacity cursor-pointer" />
            {isAuthenticated && cartCount > 0 && (
              <span className="absolute -top-2 -right-2 min-w-4 h-4 px-1 bg-primary text-white text-[10px] font-mono font-bold flex items-center justify-center">
                {cartCount}
              </span>
            )}
          </button>
        </div>
      </div>
      <nav className="hidden md:flex gap-2 md:gap-3 lg:gap-6 items-center flex-nowrap justify-center flex-1 min-w-0">
        {navCategories.map((category, i, arr) => (
          <div key={category.id} className="flex items-center gap-2 md:gap-3 lg:gap-6">
            <Link
              to={`/categories/${createSlug(category.categoryName)}`}
              className="relative group inline-block active:scale-95 transition-transform duration-150"
            >
              <span className="text-secondary font-mono text-[9px] md:text-[10px] lg:text-sm uppercase font-bold tracking-widest whitespace-nowrap block">
                {category.categoryName}
              </span>
              <span
                aria-hidden="true"
                className="absolute top-0 left-0 text-primary font-mono text-[9px] md:text-[10px] lg:text-sm uppercase font-bold tracking-widest whitespace-nowrap overflow-hidden w-0 group-hover:w-full transition-all duration-300 ease-out"
              >
                {category.categoryName}
              </span>
              <span className="absolute -bottom-1 left-0 w-full h-0.5 bg-primary origin-left scale-x-0 group-hover:scale-x-100 transition-transform duration-300 ease-out"></span>
            </Link>
            {i < arr.length - 1 && <span className="text-zinc-300">|</span>}
          </div>
        ))}
      </nav>
      <div className="hidden md:flex gap-4 lg:gap-6 items-center shrink-0">
        <form
          onSubmit={(e) => handleSearchSubmit(e, "desktop")}
          className="group relative flex items-center justify-end h-5 lg:h-6"
        >
          <button type="submit" aria-label="Search" className="z-10 focus:outline-none cursor-pointer">
            <Search className="w-4 h-4 lg:w-5 lg:h-5 hover:scale-110 transition-transform" />
          </button>
          <input
            type="text"
            name="search"
            data-search-scope="desktop"
            placeholder="Search product..."
            value={desktopSearch}
            onChange={(e) => setDesktopSearch(e.target.value)}
            className="absolute right-0 w-0 opacity-0 group-hover:w-35 lg:group-hover:w-50 group-hover:opacity-100 group-hover:border-primary focus:w-35 lg:focus:w-50 focus:opacity-100 focus:border-primary transition-all duration-300 ease-out bg-surface border-b-2 border-transparent font-mono text-[10px] lg:text-xs py-1 pr-6 lg:pr-8 outline-none z-0 cursor-text"
          />
        </form>
        {isAdmin && (
          <Link to="/admin" aria-label="Admin settings">
            <Settings className="w-4 h-4 lg:w-5 lg:h-5 hover:scale-110 transition-transform cursor-pointer" />
          </Link>
        )}
        <Link to="/profile">
          <User className="w-4 h-4 lg:w-5 lg:h-5 hover:scale-110 transition-transform cursor-pointer" />
        </Link>
        <button type="button" onClick={handleCartClick} className="relative" aria-label="Open cart">
          <ShoppingCart className="w-4 h-4 lg:w-5 lg:h-5 hover:scale-110 transition-transform cursor-pointer" />
          {isAuthenticated && cartCount > 0 && (
            <span className="absolute -top-2 -right-2 min-w-4 h-4 px-1 bg-primary text-white text-[10px] font-mono font-bold flex items-center justify-center">
              {cartCount}
            </span>
          )}
        </button>
      </div>
    </header>
  );
}